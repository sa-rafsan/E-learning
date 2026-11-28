using System.Security.Claims;
using ELearning.Application.Common;
using ELearning.Application.DTOs;
using ELearning.Application.Interfaces.Services;
using ELearning.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ELearning.Web.Controllers.Api;

[ApiController]
[Route("api/attendance")]
public sealed class AttendanceController(
    IAttendanceService attendanceService,
    ICourseService courseService) : ControllerBase
{
    [Authorize(Roles = $"{nameof(UserRole.Teacher)},{nameof(UserRole.Admin)}")]
    [HttpGet("courses")]
    public async Task<IActionResult> GetCourses(CancellationToken cancellationToken)
    {
        var courses = await courseService.GetPublishedCoursesAsync(null, cancellationToken);
        if (User.IsInRole(nameof(UserRole.Teacher)))
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            var email = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue("email");
            int.TryParse(userIdStr, out var userId);

            var db = HttpContext.RequestServices.GetRequiredService<ELearning.Infrastructure.Data.ApplicationDbContext>();
            var teacher = await db.Teachers.AsNoTracking()
                .FirstOrDefaultAsync(t => (userId > 0 && t.UserId == userId) || (!string.IsNullOrEmpty(email) && t.Email == email), cancellationToken);
            if (teacher is not null)
            {
                var directCourseIds = await db.Courses
                    .AsNoTracking()
                    .Where(c => c.TeacherId == teacher.Id)
                    .Select(c => c.Id)
                    .ToListAsync(cancellationToken);

                var assignedCourseIds = await db.CourseBatches
                    .AsNoTracking()
                    .Where(b => b.BatchTeachers.Any(bt => bt.TeacherId == teacher.Id))
                    .Select(b => b.CourseId)
                    .Distinct()
                    .ToListAsync(cancellationToken);

                courses = courses
                    .Where(c => directCourseIds.Contains(c.Id) || assignedCourseIds.Contains(c.Id))
                    .ToList();
            }
            else
            {
                courses = [];
            }
        }
        return Ok(courses);
    }

    [Authorize(Roles = $"{nameof(UserRole.Teacher)},{nameof(UserRole.Admin)}")]
    [HttpGet("roster")]
    public async Task<IActionResult> GetRoster(
        [FromQuery] int courseId,
        [FromQuery] DateOnly sessionDate,
        [FromQuery] string batch,
        [FromQuery] string section,
        CancellationToken cancellationToken)
    {
        var result = await attendanceService.GetRosterAsync(
            courseId,
            sessionDate,
            batch,
            section,
            cancellationToken);

        return ToActionResult(result);
    }

    [Authorize(Roles = nameof(UserRole.Teacher))]
    [HttpPost("roster")]
    public async Task<IActionResult> SaveRoster(
        [FromBody] SaveAttendanceRosterDto dto,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var result = await attendanceService.SaveRosterAsync(userId.Value, dto, cancellationToken);
        return ToActionResult(result);
    }

    [Authorize(Roles = nameof(UserRole.Teacher))]
    [HttpGet("sessions")]
    public async Task<IActionResult> GetTeacherSessions(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var result = await attendanceService.GetTeacherSessionsAsync(userId.Value, cancellationToken);
        return ToActionResult(result);
    }

    [Authorize(Roles = nameof(UserRole.Student))]
    [HttpGet("my")]
    public async Task<IActionResult> GetMyAttendance(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var result = await attendanceService.GetStudentAttendanceAsync(userId.Value, cancellationToken);
        return ToActionResult(result);
    }

    private int? GetCurrentUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(claim, out var id) ? id : null;
    }

    private IActionResult ToActionResult<T>(ServiceResult<T> result)
    {
        if (result.Succeeded)
        {
            return StatusCode(result.StatusCode, result.Data);
        }

        return StatusCode(result.StatusCode, new
        {
            message = result.ErrorMessage,
            errors = result.ValidationErrors
        });
    }
}

