using ELearning.Application.Common;
using ELearning.Application.DTOs;

namespace ELearning.Application.Interfaces.Services;

public interface IAuthService
{
    Task<ServiceResult<LoginResponseDto>> LoginAsync(
        LoginDto dto,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<LoginResponseDto>> RefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<bool>> LogoutAsync(
        int? userId,
        string? refreshToken,
        CancellationToken cancellationToken = default);
}

