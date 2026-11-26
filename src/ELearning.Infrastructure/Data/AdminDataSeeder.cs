using ELearning.Application.Options;
using ELearning.Domain.Enums;
using ELearning.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ELearning.Infrastructure.Data;

public static class AdminDataSeeder
{
    public static async Task SeedAsync(
        ApplicationDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<int>> roleManager,
        IOptions<AdminSettings> adminOptions,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        foreach (var roleName in Enum.GetNames<UserRole>())
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole<int>(roleName));
            }
        }

        var settings = adminOptions.Value;
        var email = settings.Email?.Trim().ToLowerInvariant() ?? string.Empty;
        var password = settings.Password ?? string.Empty;
        var fullName = string.IsNullOrWhiteSpace(settings.FullName)
            ? "Platform Administrator"
            : settings.FullName.Trim();

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            logger.LogWarning(
                "AdminSettings:Email or AdminSettings:Password is not configured. Fixed Admin account was not seeded.");
            return;
        }

        var existingAdmin = await dbContext.Users
            .SingleOrDefaultAsync(user => user.Role == UserRole.Admin, cancellationToken);

        if (existingAdmin is not null)
        {
            var needsUpdate = false;

            if (!string.Equals(existingAdmin.Email, email, StringComparison.OrdinalIgnoreCase))
            {
                existingAdmin.Email = email;
                existingAdmin.UserName = email;
                existingAdmin.NormalizedEmail = userManager.NormalizeEmail(email);
                existingAdmin.NormalizedUserName = userManager.NormalizeName(email);
                needsUpdate = true;
            }

            if (existingAdmin.Status != UserStatus.Active)
            {
                existingAdmin.Status = UserStatus.Active;
                needsUpdate = true;
            }

            if (!await userManager.CheckPasswordAsync(existingAdmin, password))
            {
                existingAdmin.PasswordHash = userManager.PasswordHasher.HashPassword(existingAdmin, password);
                needsUpdate = true;
            }

            if (needsUpdate)
            {
                existingAdmin.UpdatedAtUtc = DateTime.UtcNow;
                await userManager.UpdateAsync(existingAdmin);
                logger.LogInformation("Synchronized configured Admin account.");
            }

            if (!await userManager.IsInRoleAsync(existingAdmin, UserRole.Admin.ToString()))
            {
                await userManager.AddToRoleAsync(existingAdmin, UserRole.Admin.ToString());
            }

            return;
        }

        var emailConflict = await userManager.FindByEmailAsync(email);
        if (emailConflict is not null)
        {
            throw new InvalidOperationException(
                $"Cannot seed Admin account because a non-Admin user already uses '{email}'.");
        }

        var admin = new ApplicationUser
        {
            FullName = fullName,
            Email = email,
            UserName = email,
            EmailConfirmed = true,
            Role = UserRole.Admin,
            Status = UserStatus.Active,
            CreatedAtUtc = DateTime.UtcNow
        };

        var createResult = await userManager.CreateAsync(admin, password);
        if (!createResult.Succeeded)
        {
            var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to seed Admin account: {errors}");
        }

        await userManager.AddToRoleAsync(admin, UserRole.Admin.ToString());
        logger.LogInformation("Seeded fixed Admin account.");
    }
}
