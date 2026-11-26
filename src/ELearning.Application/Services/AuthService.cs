using ELearning.Application.Common;
using ELearning.Application.DTOs;
using ELearning.Application.Interfaces.Repositories;
using ELearning.Application.Interfaces.Services;
using ELearning.Application.Options;
using ELearning.Domain.Entities;
using ELearning.Domain.Enums;
using FluentValidation;
using Microsoft.Extensions.Options;

namespace ELearning.Application.Services;

public sealed class AuthService(
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IJwtTokenService jwtTokenService,
    IOptions<JwtSettings> jwtOptions,
    IValidator<LoginDto> loginValidator) : IAuthService
{
    private readonly JwtSettings _jwtSettings = jwtOptions.Value;

    public async Task<ServiceResult<LoginResponseDto>> LoginAsync(
        LoginDto dto,
        CancellationToken cancellationToken = default)
    {
        var validation = await loginValidator.ValidateAsync(dto, cancellationToken);
        if (!validation.IsValid)
        {
            var errors = validation.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

            return ServiceResult<LoginResponseDto>.Fail("Validation failed.", 400, errors);
        }

        if (!Enum.TryParse<UserRole>(dto.Role.Trim(), ignoreCase: true, out var selectedRole) ||
            !Enum.IsDefined(selectedRole))
        {
            return ServiceResult<LoginResponseDto>.Fail("Invalid role selected.", 400);
        }

        var user = await userRepository.GetByEmailAsync(dto.Email, cancellationToken);
        if (user is null || !await userRepository.CheckPasswordAsync(user.Id, dto.Password, cancellationToken))
        {
            return ServiceResult<LoginResponseDto>.Fail("Invalid email, password, or role.", 401);
        }

        if (user.Status != UserStatus.Active)
        {
            return ServiceResult<LoginResponseDto>.Fail(
                "Your account is inactive. Please contact an administrator.",
                403);
        }

        if (user.Role != selectedRole)
        {
            return ServiceResult<LoginResponseDto>.Fail("Invalid email, password, or role.", 401);
        }

        var (accessToken, expiresAtUtc) = jwtTokenService.GenerateAccessToken(user);
        var (rawRefreshToken, refreshTokenHash) = jwtTokenService.GenerateRefreshToken();

        var refreshDays = dto.RememberMe
            ? Math.Max(1, _jwtSettings.RememberMeRefreshTokenExpirationDays)
            : Math.Max(1, _jwtSettings.RefreshTokenExpirationDays);

        var refreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = refreshTokenHash,
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(refreshDays)
        };

        await refreshTokenRepository.AddAsync(refreshTokenEntity, cancellationToken);
        await refreshTokenRepository.SaveChangesAsync(cancellationToken);

        return ServiceResult<LoginResponseDto>.Ok(BuildLoginResponse(user, accessToken, rawRefreshToken, expiresAtUtc));
    }

    public async Task<ServiceResult<LoginResponseDto>> RefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return ServiceResult<LoginResponseDto>.Fail("Refresh token is required.", 400);
        }

        var tokenHash = jwtTokenService.HashRefreshToken(refreshToken.Trim());
        var existingToken = await refreshTokenRepository.GetByTokenHashAsync(tokenHash, cancellationToken);

        if (existingToken is null)
        {
            return ServiceResult<LoginResponseDto>.Fail("Invalid refresh token.", 401);
        }

        var now = DateTime.UtcNow;

        if (existingToken.RevokedAtUtc.HasValue)
        {
            await refreshTokenRepository.RevokeActiveTokensForUserAsync(existingToken.UserId, now, cancellationToken);
            await refreshTokenRepository.SaveChangesAsync(cancellationToken);
            return ServiceResult<LoginResponseDto>.Fail("Refresh token has been revoked.", 401);
        }

        if (existingToken.ExpiresAtUtc <= now)
        {
            existingToken.RevokedAtUtc = now;
            await refreshTokenRepository.SaveChangesAsync(cancellationToken);
            return ServiceResult<LoginResponseDto>.Fail("Refresh token has expired.", 401);
        }

        var user = await userRepository.GetByIdAsync(existingToken.UserId, cancellationToken);
        if (user is null || user.Status != UserStatus.Active)
        {
            return ServiceResult<LoginResponseDto>.Fail("User account is inactive.", 403);
        }

        var (accessToken, expiresAtUtc) = jwtTokenService.GenerateAccessToken(user);
        var (newRawRefreshToken, newRefreshTokenHash) = jwtTokenService.GenerateRefreshToken();

        existingToken.RevokedAtUtc = now;
        existingToken.ReplacedByTokenHash = newRefreshTokenHash;

        var rotatedToken = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = newRefreshTokenHash,
            CreatedAtUtc = now,
            ExpiresAtUtc = existingToken.ExpiresAtUtc
        };

        await refreshTokenRepository.AddAsync(rotatedToken, cancellationToken);
        await refreshTokenRepository.SaveChangesAsync(cancellationToken);

        return ServiceResult<LoginResponseDto>.Ok(BuildLoginResponse(user, accessToken, newRawRefreshToken, expiresAtUtc));
    }

    public async Task<ServiceResult<bool>> LogoutAsync(
        int? userId,
        string? refreshToken,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(refreshToken))
        {
            var tokenHash = jwtTokenService.HashRefreshToken(refreshToken.Trim());
            var existingToken = await refreshTokenRepository.GetByTokenHashAsync(tokenHash, cancellationToken);
            if (existingToken is not null && (!userId.HasValue || existingToken.UserId == userId.Value))
            {
                existingToken.RevokedAtUtc ??= now;
            }
        }

        if (userId.HasValue)
        {
            await refreshTokenRepository.RevokeActiveTokensForUserAsync(userId.Value, now, cancellationToken);
        }

        await refreshTokenRepository.SaveChangesAsync(cancellationToken);
        return ServiceResult<bool>.Ok(true);
    }

    private static LoginResponseDto BuildLoginResponse(
        User user,
        string accessToken,
        string refreshToken,
        DateTime expiresAtUtc)
    {
        var redirectUrl = user.Role switch
        {
            UserRole.Admin => "/Admin/Dashboard",
            UserRole.Teacher => "/Teacher/Dashboard",
            UserRole.Student => "/Student/Dashboard",
            _ => "/"
        };

        return new LoginResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAtUtc = expiresAtUtc,
            RedirectUrl = redirectUrl,
            User = new UserResponseDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role.ToString(),
                Status = user.Status.ToString(),
                StudentId = user.StudentId,
                Department = user.Department,
                CreatedAtUtc = user.CreatedAtUtc,
                UpdatedAtUtc = user.UpdatedAtUtc
            }
        };
    }
}
