using ELearning.Application.Common;
using ELearning.Application.DTOs;
using ELearning.Application.Interfaces.Repositories;
using ELearning.Application.Interfaces.Services;
using ELearning.Domain.Entities;
using ELearning.Domain.Enums;
using FluentValidation;
using FluentValidation.Results;

namespace ELearning.Application.Services;

public sealed class UserService(
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IValidator<CreateTeacherDto> createTeacherValidator,
    IValidator<CreateStudentDto> createStudentValidator,
    IValidator<UpdateTeacherDto> updateTeacherValidator,
    IValidator<UpdateStudentDto> updateStudentValidator,
    IValidator<UpdateUserStatusDto> updateStatusValidator,
    IValidator<ResetPasswordDto> resetPasswordValidator) : IUserService
{
    public async Task<ServiceResult<UserResponseDto>> CreateTeacherAsync(
        CreateTeacherDto dto,
        CancellationToken cancellationToken = default)
    {
        var validation = await createTeacherValidator.ValidateAsync(dto, cancellationToken);
        if (!validation.IsValid)
        {
            return ValidationFailure<UserResponseDto>(validation);
        }

        var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
        if (await userRepository.GetByEmailAsync(normalizedEmail, cancellationToken) is not null)
        {
            return ServiceResult<UserResponseDto>.Fail("An account with this email already exists.", 409);
        }

        var teacher = new User
        {
            FullName = dto.FullName.Trim(),
            Email = normalizedEmail,
            PhoneNumber = NormalizeOptional(dto.PhoneNumber),
            Role = UserRole.Teacher,
            Status = dto.Status,
            CreatedAtUtc = DateTime.UtcNow
        };

        var (succeeded, createdUser, errorMessage) =
            await userRepository.CreateUserAsync(teacher, dto.Password, cancellationToken);

        if (!succeeded || createdUser is null)
        {
            return ServiceResult<UserResponseDto>.Fail(errorMessage ?? "Unable to create teacher account.", 400);
        }

        return ServiceResult<UserResponseDto>.Ok(MapToDto(createdUser), 201);
    }

    public async Task<ServiceResult<UserResponseDto>> CreateStudentAsync(
        CreateStudentDto dto,
        CancellationToken cancellationToken = default)
    {
        var validation = await createStudentValidator.ValidateAsync(dto, cancellationToken);
        if (!validation.IsValid)
        {
            return ValidationFailure<UserResponseDto>(validation);
        }

        var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
        if (await userRepository.GetByEmailAsync(normalizedEmail, cancellationToken) is not null)
        {
            return ServiceResult<UserResponseDto>.Fail("An account with this email already exists.", 409);
        }

        var normalizedStudentId = dto.StudentId.Trim();
        if (await userRepository.GetByStudentIdAsync(normalizedStudentId, cancellationToken) is not null)
        {
            return ServiceResult<UserResponseDto>.Fail("A student with this Student ID already exists.", 409);
        }

        var student = new User
        {
            FullName = dto.FullName.Trim(),
            Email = normalizedEmail,
            PhoneNumber = NormalizeOptional(dto.PhoneNumber),
            StudentId = normalizedStudentId,
            Department = NormalizeOptional(dto.Department),
            Role = UserRole.Student,
            Status = dto.Status,
            CreatedAtUtc = DateTime.UtcNow
        };

        var (succeeded, createdUser, errorMessage) =
            await userRepository.CreateUserAsync(student, dto.Password, cancellationToken);

        if (!succeeded || createdUser is null)
        {
            return ServiceResult<UserResponseDto>.Fail(errorMessage ?? "Unable to create student account.", 400);
        }

        return ServiceResult<UserResponseDto>.Ok(MapToDto(createdUser), 201);
    }

    public async Task<ServiceResult<IReadOnlyList<UserResponseDto>>> GetUsersAsync(
        UserRole? role,
        string? search,
        CancellationToken cancellationToken = default)
    {
        var users = await userRepository.GetUsersAsync(role, search, cancellationToken);
        return ServiceResult<IReadOnlyList<UserResponseDto>>.Ok(users.Select(MapToDto).ToList());
    }

    public Task<ServiceResult<IReadOnlyList<UserResponseDto>>> GetTeachersAsync(
        string? search,
        CancellationToken cancellationToken = default)
    {
        return GetUsersAsync(UserRole.Teacher, search, cancellationToken);
    }

    public Task<ServiceResult<IReadOnlyList<UserResponseDto>>> GetStudentsAsync(
        string? search,
        CancellationToken cancellationToken = default)
    {
        return GetUsersAsync(UserRole.Student, search, cancellationToken);
    }

    public async Task<ServiceResult<UserResponseDto>> GetUserByIdAsync(
        int id,
        int? requestingUserId = null,
        bool isAdmin = true,
        CancellationToken cancellationToken = default)
    {
        if (!isAdmin && requestingUserId != id)
        {
            return ServiceResult<UserResponseDto>.Fail("You are not authorized to access this user.", 403);
        }

        var user = await userRepository.GetByIdAsync(id, cancellationToken);
        if (user is null)
        {
            return ServiceResult<UserResponseDto>.Fail("User not found.", 404);
        }

        return ServiceResult<UserResponseDto>.Ok(MapToDto(user));
    }

    public async Task<ServiceResult<UserResponseDto>> UpdateTeacherAsync(
        int id,
        UpdateTeacherDto dto,
        CancellationToken cancellationToken = default)
    {
        var validation = await updateTeacherValidator.ValidateAsync(dto, cancellationToken);
        if (!validation.IsValid)
        {
            return ValidationFailure<UserResponseDto>(validation);
        }

        var teacher = await userRepository.GetByIdAsync(id, cancellationToken);
        if (teacher is null || teacher.Role != UserRole.Teacher)
        {
            return ServiceResult<UserResponseDto>.Fail("Teacher account not found.", 404);
        }

        var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
        var existingByEmail = await userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);
        if (existingByEmail is not null && existingByEmail.Id != id)
        {
            return ServiceResult<UserResponseDto>.Fail("An account with this email already exists.", 409);
        }

        teacher.FullName = dto.FullName.Trim();
        teacher.Email = normalizedEmail;
        teacher.PhoneNumber = NormalizeOptional(dto.PhoneNumber);
        teacher.Status = dto.Status;
        teacher.UpdatedAtUtc = DateTime.UtcNow;

        var (succeeded, updatedUser, errorMessage) = await userRepository.UpdateUserAsync(teacher, cancellationToken);
        if (!succeeded || updatedUser is null)
        {
            return ServiceResult<UserResponseDto>.Fail(errorMessage ?? "Unable to update teacher account.", 400);
        }

        if (teacher.Status == UserStatus.Inactive)
        {
            await refreshTokenRepository.RevokeActiveTokensForUserAsync(teacher.Id, DateTime.UtcNow, cancellationToken);
            await refreshTokenRepository.SaveChangesAsync(cancellationToken);
        }

        return ServiceResult<UserResponseDto>.Ok(MapToDto(updatedUser));
    }

    public async Task<ServiceResult<UserResponseDto>> UpdateStudentAsync(
        int id,
        UpdateStudentDto dto,
        CancellationToken cancellationToken = default)
    {
        var validation = await updateStudentValidator.ValidateAsync(dto, cancellationToken);
        if (!validation.IsValid)
        {
            return ValidationFailure<UserResponseDto>(validation);
        }

        var student = await userRepository.GetByIdAsync(id, cancellationToken);
        if (student is null || student.Role != UserRole.Student)
        {
            return ServiceResult<UserResponseDto>.Fail("Student account not found.", 404);
        }

        var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
        var existingByEmail = await userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);
        if (existingByEmail is not null && existingByEmail.Id != id)
        {
            return ServiceResult<UserResponseDto>.Fail("An account with this email already exists.", 409);
        }

        var normalizedStudentId = dto.StudentId.Trim();
        var existingByStudentId = await userRepository.GetByStudentIdAsync(normalizedStudentId, cancellationToken);
        if (existingByStudentId is not null && existingByStudentId.Id != id)
        {
            return ServiceResult<UserResponseDto>.Fail("A student with this Student ID already exists.", 409);
        }

        student.FullName = dto.FullName.Trim();
        student.Email = normalizedEmail;
        student.PhoneNumber = NormalizeOptional(dto.PhoneNumber);
        student.StudentId = normalizedStudentId;
        student.Department = NormalizeOptional(dto.Department);
        student.Status = dto.Status;
        student.UpdatedAtUtc = DateTime.UtcNow;

        var (succeeded, updatedUser, errorMessage) = await userRepository.UpdateUserAsync(student, cancellationToken);
        if (!succeeded || updatedUser is null)
        {
            return ServiceResult<UserResponseDto>.Fail(errorMessage ?? "Unable to update student account.", 400);
        }

        if (student.Status == UserStatus.Inactive)
        {
            await refreshTokenRepository.RevokeActiveTokensForUserAsync(student.Id, DateTime.UtcNow, cancellationToken);
            await refreshTokenRepository.SaveChangesAsync(cancellationToken);
        }

        return ServiceResult<UserResponseDto>.Ok(MapToDto(updatedUser));
    }

    public async Task<ServiceResult<UserResponseDto>> UpdateStatusAsync(
        int id,
        UpdateUserStatusDto dto,
        int currentAdminId,
        CancellationToken cancellationToken = default)
    {
        var validation = await updateStatusValidator.ValidateAsync(dto, cancellationToken);
        if (!validation.IsValid)
        {
            return ValidationFailure<UserResponseDto>(validation);
        }

        var user = await userRepository.GetByIdAsync(id, cancellationToken);
        if (user is null)
        {
            return ServiceResult<UserResponseDto>.Fail("User not found.", 404);
        }

        if (user.Role == UserRole.Admin || user.Id == currentAdminId)
        {
            return ServiceResult<UserResponseDto>.Fail("The Admin account status cannot be modified.", 400);
        }

        user.Status = dto.Status;
        user.UpdatedAtUtc = DateTime.UtcNow;

        var (succeeded, updatedUser, errorMessage) = await userRepository.UpdateUserAsync(user, cancellationToken);
        if (!succeeded || updatedUser is null)
        {
            return ServiceResult<UserResponseDto>.Fail(errorMessage ?? "Unable to update user status.", 400);
        }

        if (dto.Status == UserStatus.Inactive)
        {
            await refreshTokenRepository.RevokeActiveTokensForUserAsync(user.Id, DateTime.UtcNow, cancellationToken);
            await refreshTokenRepository.SaveChangesAsync(cancellationToken);
        }

        return ServiceResult<UserResponseDto>.Ok(MapToDto(updatedUser));
    }

    public async Task<ServiceResult<bool>> ResetPasswordAsync(
        int id,
        ResetPasswordDto dto,
        CancellationToken cancellationToken = default)
    {
        var validation = await resetPasswordValidator.ValidateAsync(dto, cancellationToken);
        if (!validation.IsValid)
        {
            return ValidationFailure<bool>(validation);
        }

        var user = await userRepository.GetByIdAsync(id, cancellationToken);
        if (user is null)
        {
            return ServiceResult<bool>.Fail("User not found.", 404);
        }

        if (user.Role == UserRole.Admin)
        {
            return ServiceResult<bool>.Fail("Admin password is managed through backend configuration.", 400);
        }

        var (succeeded, errorMessage) = await userRepository.ResetPasswordAsync(id, dto.NewPassword, cancellationToken);
        if (!succeeded)
        {
            return ServiceResult<bool>.Fail(errorMessage ?? "Unable to reset password.", 400);
        }

        await refreshTokenRepository.RevokeActiveTokensForUserAsync(user.Id, DateTime.UtcNow, cancellationToken);
        await refreshTokenRepository.SaveChangesAsync(cancellationToken);

        return ServiceResult<bool>.Ok(true);
    }

    private static ServiceResult<T> ValidationFailure<T>(ValidationResult validation)
    {
        var errors = validation.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

        return ServiceResult<T>.Fail("Validation failed.", 400, errors);
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static UserResponseDto MapToDto(User user) => new()
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
    };
}
