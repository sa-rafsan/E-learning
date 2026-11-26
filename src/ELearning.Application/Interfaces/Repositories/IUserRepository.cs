using ELearning.Domain.Entities;
using ELearning.Domain.Enums;

namespace ELearning.Application.Interfaces.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<User?> GetByStudentIdAsync(string studentId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<User>> GetUsersAsync(
        UserRole? role,
        string? search,
        CancellationToken cancellationToken = default);

    Task<(bool Succeeded, User? User, string? ErrorMessage)> CreateUserAsync(
        User user,
        string password,
        CancellationToken cancellationToken = default);

    Task<bool> CheckPasswordAsync(
        int userId,
        string password,
        CancellationToken cancellationToken = default);

    Task<(bool Succeeded, User? User, string? ErrorMessage)> UpdateUserAsync(
        User user,
        CancellationToken cancellationToken = default);

    Task<(bool Succeeded, string? ErrorMessage)> ResetPasswordAsync(
        int userId,
        string newPassword,
        CancellationToken cancellationToken = default);
}
