using System.Security.Claims;
using ELearning.Application.Common;
using ELearning.Application.DTOs;
using ELearning.Application.Interfaces.Services;
using ELearning.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

