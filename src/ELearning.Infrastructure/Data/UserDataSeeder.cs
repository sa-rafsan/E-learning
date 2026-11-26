using ELearning.Domain.Entities;
using ELearning.Domain.Enums;
using ELearning.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ELearning.Infrastructure.Data;

public static class UserDataSeeder
{
    private const long StartingStudentCode = 325704650;
    private const int TotalStudentsToSeed = 100;
    private const int StudentsPerSection = 30;
    private const int BatchNumber = 70;

    public static async Task SeedAsync(
        ApplicationDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        await SeedTeachersAsync(dbContext, userManager, logger, cancellationToken);
        await SeedStudentsAsync(dbContext, userManager, logger, cancellationToken);
    }

    private static async Task SeedTeachersAsync(
        ApplicationDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        if (await dbContext.Teachers.AnyAsync(cancellationToken))
        {
            return;
        }

        var teachers = new[]
        {
            new { FullName = "Ayesha Rahman", Email = "ayesha.rahman@wub.edu.bd", Phone = "01711000001", Department = "Computer Science", Designation = "Senior Lecturer" },
            new { FullName = "Marcus Chen", Email = "marcus.chen@wub.edu.bd", Phone = "01711000002", Department = "Software Engineering", Designation = "Assistant Professor" },
            new { FullName = "Nadia Williams", Email = "nadia.williams@wub.edu.bd", Phone = "01711000003", Department = "Data Science", Designation = "Lecturer" },
            new { FullName = "Tanvir Ahmed", Email = "tanvir.ahmed@wub.edu.bd", Phone = "01711000004", Department = "Computer Science", Designation = "Associate Professor" },
            new { FullName = "Farhana Islam", Email = "farhana.islam@wub.edu.bd", Phone = "01711000005", Department = "Information Technology", Designation = "Lecturer" }
        };

        var seededCount = 0;
        foreach (var item in teachers)
        {
            var appUser = await userManager.FindByEmailAsync(item.Email);
            if (appUser is null)
            {
                appUser = new ApplicationUser
                {
                    FullName = item.FullName,
                    Email = item.Email,
                    UserName = item.Email,
                    PhoneNumber = item.Phone,
                    Department = item.Department,
                    EmailConfirmed = true,
                    Role = UserRole.Teacher,
                    Status = UserStatus.Active,
                    CreatedAtUtc = DateTime.UtcNow
                };

                var createResult = await userManager.CreateAsync(appUser, "Teacher@123");
                if (!createResult.Succeeded)
                {
                    var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Failed to seed teacher '{item.Email}': {errors}");
                }

                await userManager.AddToRoleAsync(appUser, UserRole.Teacher.ToString());
            }

            await dbContext.Teachers.AddAsync(new Teacher
            {
                UserId = appUser.Id,
                FullName = item.FullName,
                Email = item.Email,
                PhoneNumber = item.Phone,
                Department = item.Department,
                Designation = item.Designation,
                CreatedAtUtc = appUser.CreatedAtUtc
            }, cancellationToken);

            seededCount++;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Seeded {Count} Teacher records.", seededCount);
    }

    private static async Task SeedStudentsAsync(
        ApplicationDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        if (await dbContext.Students.AnyAsync(cancellationToken))
        {
            return;
        }

        for (var i = 0; i < TotalStudentsToSeed; i++)
        {
            var code = (StartingStudentCode + i).ToString("D10");
            var section = ((char)('A' + (i / StudentsPerSection))).ToString();
            var email = $"{code}@student.wub.edu.bd";
            var department = $"Batch {BatchNumber} - Section {section}";
            var fullName = $"Student {i + 1:D3} (Batch {BatchNumber}{section})";

            var appUser = await userManager.FindByEmailAsync(email);
            if (appUser is null)
            {
                appUser = new ApplicationUser
                {
                    FullName = fullName,
                    Email = email,
                    UserName = email,
                    PhoneNumber = code,
                    StudentId = code,
                    Department = department,
                    EmailConfirmed = true,
                    Role = UserRole.Student,
                    Status = UserStatus.Active,
                    CreatedAtUtc = DateTime.UtcNow
                };

                var createResult = await userManager.CreateAsync(appUser, code);
                if (!createResult.Succeeded)
                {
                    var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Failed to seed student '{email}': {errors}");
                }

                await userManager.AddToRoleAsync(appUser, UserRole.Student.ToString());
            }

            await dbContext.Students.AddAsync(new Student
            {
                UserId = appUser.Id,
                StudentId = code,
                FullName = fullName,
                Email = email,
                PhoneNumber = code,
                Batch = BatchNumber.ToString(),
                Section = section,
                Department = department,
                CreatedAtUtc = appUser.CreatedAtUtc
            }, cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Seeded {Count} Student records for Batch {Batch}.", TotalStudentsToSeed, BatchNumber);
    }
}
