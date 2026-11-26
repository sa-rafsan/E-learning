using ELearning.Application.Common;
using ELearning.Application.DTOs;
using ELearning.Domain.Enums;

namespace ELearning.Application.Interfaces.Services;

public interface IUserService
{
    Task<ServiceResult<UserResponseDto>> CreateTeacherAsync(
        CreateTeacherDto dto,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<UserResponseDto>> CreateStudentAsync(
        CreateStudentDto dto,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<IReadOnlyList<UserResponseDto>>> GetUsersAsync(
        UserRole? role,
        string? search,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<IReadOnlyList<UserResponseDto>>> GetTeachersAsync(
        string? search,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<IReadOnlyList<UserResponseDto>>> GetStudentsAsync(
        string? search,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<UserResponseDto>> GetUserByIdAsync(
        int id,
        int? requestingUserId = null,
        bool isAdmin = true,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<UserResponseDto>> UpdateTeacherAsync(
        int id,
        UpdateTeacherDto dto,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<UserResponseDto>> UpdateStudentAsync(
        int id,
        UpdateStudentDto dto,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<UserResponseDto>> UpdateStatusAsync(
        int id,
        UpdateUserStatusDto dto,
        int currentAdminId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<bool>> ResetPasswordAsync(
        int id,
        ResetPasswordDto dto,
        CancellationToken cancellationToken = default);
}

