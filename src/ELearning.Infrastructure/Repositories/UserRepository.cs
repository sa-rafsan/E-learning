using ELearning.Application.Interfaces.Repositories;
using ELearning.Domain.Entities;
using ELearning.Domain.Enums;
using ELearning.Infrastructure.Data;
using ELearning.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ELearning.Infrastructure.Repositories;

public sealed class UserRepository(
    ApplicationDbContext dbContext,
    UserManager<ApplicationUser> userManager)
    : GenericRepository<ApplicationUser>(dbContext), IUserRepository
{
    public new async Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var appUser = await DbSet.AsNoTracking()
            .Include(user => user.TeacherProfile)
            .Include(user => user.StudentProfile)
            .SingleOrDefaultAsync(user => user.Id == id, cancellationToken);

        return appUser is null ? null : MapToDomain(appUser);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalized = userManager.NormalizeEmail(email.Trim());
        var appUser = await DbSet.AsNoTracking()
            .Include(user => user.TeacherProfile)
            .Include(user => user.StudentProfile)
            .SingleOrDefaultAsync(user => user.NormalizedEmail == normalized, cancellationToken);

        return appUser is null ? null : MapToDomain(appUser);
    }

    public async Task<User?> GetByStudentIdAsync(string studentId, CancellationToken cancellationToken = default)
    {
        var normalized = studentId.Trim();
        var appUser = await DbSet.AsNoTracking()
            .Include(user => user.StudentProfile)
            .SingleOrDefaultAsync(
                user => (user.StudentProfile != null && user.StudentProfile.StudentId == normalized) ||
                        (user.StudentId != null && user.StudentId == normalized),
                cancellationToken);

        return appUser is null ? null : MapToDomain(appUser);
    }

    public async Task<IReadOnlyList<User>> GetUsersAsync(
        UserRole? role,
        string? search,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsNoTracking()
            .Include(user => user.TeacherProfile)
            .Include(user => user.StudentProfile)
            .AsQueryable();

        if (role.HasValue)
        {
            query = query.Where(user => user.Role == role.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchTerm = search.Trim()
                .Replace("\\", "\\\\", StringComparison.Ordinal)
                .Replace("%", "\\%", StringComparison.Ordinal)
                .Replace("_", "\\_", StringComparison.Ordinal);
            var pattern = $"%{searchTerm}%";

            query = query.Where(user =>
                EF.Functions.Like(user.FullName, pattern, "\\") ||
                (user.Email != null && EF.Functions.Like(user.Email, pattern, "\\")) ||
                (user.PhoneNumber != null && EF.Functions.Like(user.PhoneNumber, pattern, "\\")) ||
                (user.StudentId != null && EF.Functions.Like(user.StudentId, pattern, "\\")) ||
                (user.Department != null && EF.Functions.Like(user.Department, pattern, "\\")) ||
                (user.StudentProfile != null &&
                 (EF.Functions.Like(user.StudentProfile.StudentId, pattern, "\\") ||
                  (user.StudentProfile.Department != null && EF.Functions.Like(user.StudentProfile.Department, pattern, "\\")) ||
                  (user.StudentProfile.Batch != null && EF.Functions.Like(user.StudentProfile.Batch, pattern, "\\")) ||
                  (user.StudentProfile.Section != null && EF.Functions.Like(user.StudentProfile.Section, pattern, "\\")))));
        }

        var users = await query
            .OrderBy(user => user.Role)
            .ThenBy(user => user.FullName)
            .ToListAsync(cancellationToken);

        return users.Select(MapToDomain).ToList();
    }

    public async Task<(bool Succeeded, User? User, string? ErrorMessage)> CreateUserAsync(
        User user,
        string password,
        CancellationToken cancellationToken = default)
    {
        var appUser = new ApplicationUser
        {
            FullName = user.FullName,
            Email = user.Email,
            UserName = user.Email,
            EmailConfirmed = true,
            PhoneNumber = user.PhoneNumber,
            Role = user.Role,
            Status = user.Status,
            StudentId = user.StudentId,
            Department = user.Department,
            CreatedAtUtc = user.CreatedAtUtc
        };

        var createResult = await userManager.CreateAsync(appUser, password);
        if (!createResult.Succeeded)
        {
            return (false, null, string.Join(" ", createResult.Errors.Select(e => e.Description)));
        }

        var roleResult = await userManager.AddToRoleAsync(appUser, user.Role.ToString());
        if (!roleResult.Succeeded)
        {
            await userManager.DeleteAsync(appUser);
            return (false, null, string.Join(" ", roleResult.Errors.Select(e => e.Description)));
        }

        if (user.Role == UserRole.Teacher)
        {
            var teacherProfile = new Teacher
            {
                UserId = appUser.Id,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Department = user.Department,
                ImagePath = user.ImagePath,
                CreatedAtUtc = user.CreatedAtUtc
            };
            await DbContext.Teachers.AddAsync(teacherProfile, cancellationToken);
            await DbContext.SaveChangesAsync(cancellationToken);
            appUser.TeacherProfile = teacherProfile;
        }
        else if (user.Role == UserRole.Student)
        {
            var studentProfile = new Student
            {
                UserId = appUser.Id,
                StudentId = user.StudentId ?? string.Empty,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Department = user.Department,
                ImagePath = user.ImagePath,
                CreatedAtUtc = user.CreatedAtUtc
            };
            await DbContext.Students.AddAsync(studentProfile, cancellationToken);
            await DbContext.SaveChangesAsync(cancellationToken);
            appUser.StudentProfile = studentProfile;
        }

        return (true, MapToDomain(appUser), null);
    }

    public async Task<bool> CheckPasswordAsync(
        int userId,
        string password,
        CancellationToken cancellationToken = default)
    {
        var appUser = await userManager.FindByIdAsync(userId.ToString());
        if (appUser is null)
        {
            return false;
        }

        return await userManager.CheckPasswordAsync(appUser, password);
    }

    public async Task<(bool Succeeded, User? User, string? ErrorMessage)> UpdateUserAsync(
        User user,
        CancellationToken cancellationToken = default)
    {
        var appUser = await userManager.FindByIdAsync(user.Id.ToString());
        if (appUser is null)
        {
            return (false, null, "User not found.");
        }

        var now = user.UpdatedAtUtc ?? DateTime.UtcNow;
        appUser.FullName = user.FullName;
        appUser.Email = user.Email;
        appUser.UserName = user.Email;
        appUser.PhoneNumber = user.PhoneNumber;
        appUser.StudentId = user.StudentId;
        appUser.Department = user.Department;
        appUser.Status = user.Status;
        appUser.UpdatedAtUtc = now;

        var result = await userManager.UpdateAsync(appUser);
        if (!result.Succeeded)
        {
            return (false, null, string.Join(" ", result.Errors.Select(e => e.Description)));
        }

        if (appUser.Role == UserRole.Teacher)
        {
            var teacherProfile = await DbContext.Teachers
                .SingleOrDefaultAsync(t => t.UserId == appUser.Id, cancellationToken);

            if (teacherProfile is not null)
            {
                teacherProfile.FullName = user.FullName;
                teacherProfile.Email = user.Email;
                teacherProfile.PhoneNumber = user.PhoneNumber;
                teacherProfile.Department = user.Department;
                if (user.ImagePath is not null)
                {
                    teacherProfile.ImagePath = user.ImagePath;
                }
                teacherProfile.UpdatedAtUtc = now;
                await DbContext.SaveChangesAsync(cancellationToken);
                appUser.TeacherProfile = teacherProfile;
            }
        }
        else if (appUser.Role == UserRole.Student)
        {
            var studentProfile = await DbContext.Students
                .SingleOrDefaultAsync(s => s.UserId == appUser.Id, cancellationToken);

            if (studentProfile is not null)
            {
                studentProfile.StudentId = user.StudentId ?? studentProfile.StudentId;
                studentProfile.FullName = user.FullName;
                studentProfile.Email = user.Email;
                studentProfile.PhoneNumber = user.PhoneNumber;
                studentProfile.Department = user.Department;
                if (user.ImagePath is not null)
                {
                    studentProfile.ImagePath = user.ImagePath;
                }
                studentProfile.UpdatedAtUtc = now;
                await DbContext.SaveChangesAsync(cancellationToken);
                appUser.StudentProfile = studentProfile;
            }
        }

        return (true, MapToDomain(appUser), null);
    }

    public async Task<(bool Succeeded, string? ErrorMessage)> ResetPasswordAsync(
        int userId,
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        var appUser = await userManager.FindByIdAsync(userId.ToString());
        if (appUser is null)
        {
            return (false, "User not found.");
        }

        var token = await userManager.GeneratePasswordResetTokenAsync(appUser);
        var result = await userManager.ResetPasswordAsync(appUser, token, newPassword);
        if (!result.Succeeded)
        {
            return (false, string.Join(" ", result.Errors.Select(e => e.Description)));
        }

        appUser.UpdatedAtUtc = DateTime.UtcNow;
        await userManager.UpdateAsync(appUser);

        return (true, null);
    }

    private static User MapToDomain(ApplicationUser appUser) => new()
    {
        Id = appUser.Id,
        FullName = appUser.StudentProfile?.FullName ?? appUser.TeacherProfile?.FullName ?? appUser.FullName,
        Email = appUser.StudentProfile?.Email ?? appUser.TeacherProfile?.Email ?? appUser.Email ?? string.Empty,
        PhoneNumber = appUser.StudentProfile?.PhoneNumber ?? appUser.TeacherProfile?.PhoneNumber ?? appUser.PhoneNumber,
        Role = appUser.Role,
        Status = appUser.Status,
        StudentId = appUser.StudentProfile?.StudentId ?? appUser.StudentId,
        Department = appUser.StudentProfile?.Department ?? appUser.TeacherProfile?.Department ?? appUser.Department,
        ImagePath = appUser.StudentProfile?.ImagePath ?? appUser.TeacherProfile?.ImagePath,
        CreatedAtUtc = appUser.CreatedAtUtc,
        UpdatedAtUtc = appUser.UpdatedAtUtc
    };
}
