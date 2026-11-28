using System.Security.Claims;
using ELearning.Application.DTOs;
using ELearning.Domain.Entities;
using ELearning.Domain.Enums;
using ELearning.Infrastructure.Data;
using ELearning.Infrastructure.Identity;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ELearning.Web.Controllers.Api;

[ApiController]
[Authorize]
[Route("api/account")]
public sealed class ProfileApiController(
    UserManager<ApplicationUser> userManager,
    ApplicationDbContext dbContext,
    IWebHostEnvironment environment,
    IValidator<ChangePasswordDto> changePasswordValidator) : ControllerBase
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp"
    };

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var user = await dbContext.Users
            .AsNoTracking()
            .Include(u => u.TeacherProfile)
                .ThenInclude(t => t!.Qualifications)
            .Include(u => u.StudentProfile)
            .SingleOrDefaultAsync(u => u.Id == userId.Value, cancellationToken);

        if (user is null)
        {
            return NotFound();
        }

        return Ok(new
        {
            id = user.Id,
            fullName = user.StudentProfile?.FullName ?? user.TeacherProfile?.FullName ?? user.FullName,
            email = user.Email,
            phoneNumber = user.StudentProfile?.PhoneNumber ?? user.TeacherProfile?.PhoneNumber ?? user.PhoneNumber,
            role = user.Role.ToString(),
            status = user.Status.ToString(),
            studentId = user.StudentProfile?.StudentId ?? user.StudentId,
            department = user.StudentProfile?.Department ?? user.TeacherProfile?.Department ?? user.Department,
            imagePath = user.StudentProfile?.ImagePath ?? user.TeacherProfile?.ImagePath,
            address = user.StudentProfile?.Address ?? user.TeacherProfile?.Address,
            nationality = user.StudentProfile?.Nationality ?? user.TeacherProfile?.Nationality,
            country = user.StudentProfile?.Country ?? user.TeacherProfile?.Country,
            dateOfBirth = user.StudentProfile?.DateOfBirth,
            gender = user.StudentProfile?.Gender,
            educationalBackground = user.StudentProfile?.EducationalBackground,
            specialization = user.TeacherProfile?.Specialization,
            professionalExperience = user.TeacherProfile?.ProfessionalExperience,
            shortBiography = user.TeacherProfile?.ShortBiography,
            qualifications = user.TeacherProfile?.Qualifications.Select(q => new
            {
                q.Id,
                q.DegreeName,
                q.InstitutionName,
                q.PassingYear
            })
        });
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile(
        [FromBody] UpdateSelfProfileDto dto,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dto.FullName))
        {
            return BadRequest(new { message = "Full Name is required." });
        }

        var userId = GetCurrentUserId();
        if (!userId.HasValue) return Unauthorized();

        var user = await dbContext.Users
            .Include(u => u.TeacherProfile)
                .ThenInclude(t => t!.Qualifications)
            .Include(u => u.StudentProfile)
            .SingleOrDefaultAsync(u => u.Id == userId.Value, cancellationToken);

        if (user is null) return NotFound();

        var now = DateTime.UtcNow;
        user.FullName = dto.FullName.Trim();
        user.PhoneNumber = dto.PhoneNumber?.Trim();
        user.UpdatedAtUtc = now;

        if (user.TeacherProfile is not null)
        {
            var t = user.TeacherProfile;
            t.FullName = user.FullName;
            t.PhoneNumber = user.PhoneNumber;
            t.Address = dto.Address?.Trim();
            t.Nationality = dto.Nationality?.Trim();
            t.Country = dto.Country?.Trim();
            t.Specialization = dto.Specialization?.Trim();
            t.ProfessionalExperience = dto.ProfessionalExperience?.Trim();
            t.ShortBiography = dto.ShortBiography?.Trim();
            t.UpdatedAtUtc = now;

            if (dto.Qualifications is not null)
            {
                t.Qualifications.Clear();
                foreach (var q in dto.Qualifications.Where(q => !string.IsNullOrWhiteSpace(q.DegreeName)))
                {
                    t.Qualifications.Add(new TeacherQualification
                    {
                        TeacherId = t.Id,
                        DegreeName = q.DegreeName.Trim(),
                        InstitutionName = q.InstitutionName.Trim(),
                        PassingYear = q.PassingYear
                    });
                }
            }
        }
        else if (user.StudentProfile is not null)
        {
            var s = user.StudentProfile;
            s.FullName = user.FullName;
            s.PhoneNumber = user.PhoneNumber;
            s.Address = dto.Address?.Trim();
            s.Nationality = dto.Nationality?.Trim();
            s.Country = dto.Country?.Trim();
            s.DateOfBirth = dto.DateOfBirth;
            s.Gender = dto.Gender?.Trim();
            s.EducationalBackground = dto.EducationalBackground?.Trim();
            s.UpdatedAtUtc = now;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(new { message = "Profile updated successfully." });
    }

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordDto dto,
        CancellationToken cancellationToken)
    {
        var validation = await changePasswordValidator.ValidateAsync(dto, cancellationToken);
        if (!validation.IsValid)
        {
            var errors = validation.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

            return BadRequest(new { message = "Validation failed.", errors });
        }

        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var appUser = await userManager.FindByIdAsync(userId.Value.ToString());
        if (appUser is null)
        {
            return NotFound(new { message = "User not found." });
        }

        var result = await userManager.ChangePasswordAsync(appUser, dto.CurrentPassword, dto.NewPassword);
        if (!result.Succeeded)
        {
            var err = string.Join(" ", result.Errors.Select(e => e.Description));
            return BadRequest(new { message = err });
        }

        appUser.UpdatedAtUtc = DateTime.UtcNow;
        await userManager.UpdateAsync(appUser);

        return Ok(new { message = "Password updated successfully." });
    }

    [HttpPost("profile/photo")]
    public async Task<IActionResult> UploadProfilePhoto(
        IFormFile? file,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        if (file is null || file.Length == 0 || file.Length > 5 * 1024 * 1024)
        {
            return BadRequest(new { message = "Please upload a valid image up to 5 MB." });
        }

        var ext = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(ext) || !AllowedExtensions.Contains(ext))
        {
            return BadRequest(new { message = "Only .jpg, .jpeg, .png, and .webp images are allowed." });
        }

        var user = await dbContext.Users
            .Include(u => u.TeacherProfile)
            .Include(u => u.StudentProfile)
            .SingleOrDefaultAsync(u => u.Id == userId.Value, cancellationToken);

        if (user is null)
        {
            return NotFound();
        }

        var folder = user.Role == UserRole.Teacher ? "teachers" : "students";
        var webRoot = environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot");
        var uploadDir = Path.Combine(webRoot, "uploads", folder);
        Directory.CreateDirectory(uploadDir);

        var fileName = $"{Guid.NewGuid():N}{ext.ToLowerInvariant()}";
        var fullPath = Path.Combine(uploadDir, fileName);

        await using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        var relativePath = $"/uploads/{folder}/{fileName}";
        if (user.TeacherProfile is not null)
        {
            user.TeacherProfile.ImagePath = relativePath;
            user.TeacherProfile.UpdatedAtUtc = DateTime.UtcNow;
        }
        else if (user.StudentProfile is not null)
        {
            user.StudentProfile.ImagePath = relativePath;
            user.StudentProfile.UpdatedAtUtc = DateTime.UtcNow;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(new { imagePath = relativePath });
    }

    private int? GetCurrentUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(claim, out var id) ? id : null;
    }
}
