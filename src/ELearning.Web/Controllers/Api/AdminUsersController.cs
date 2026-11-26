using System.Security.Claims;
using ELearning.Application.Common;
using ELearning.Application.DTOs;
using ELearning.Application.Interfaces.Services;
using ELearning.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ELearning.Web.Controllers.Api;

[ApiController]
[Authorize(Roles = nameof(UserRole.Admin))]
[Route("api/admin/users")]
public sealed class AdminUsersController(IUserService userService) : ControllerBase
{
    [HttpPost("teacher")]
    public async Task<IActionResult> CreateTeacher(
        [FromBody] CreateTeacherDto dto,
        CancellationToken cancellationToken)
    {
        var result = await userService.CreateTeacherAsync(dto, cancellationToken);
        return ToActionResult(result);
    }

    [HttpPost("student")]
    public async Task<IActionResult> CreateStudent(
        [FromBody] CreateStudentDto dto,
        CancellationToken cancellationToken)
    {
        var result = await userService.CreateStudentAsync(dto, cancellationToken);
        return ToActionResult(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers(
        [FromQuery] string? role,
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        UserRole? parsedRole = null;
        if (!string.IsNullOrWhiteSpace(role))
        {
            if (!Enum.TryParse<UserRole>(role.Trim(), ignoreCase: true, out var r) || !Enum.IsDefined(r))
            {
                return BadRequest(new { message = "Invalid role filter." });
            }

            parsedRole = r;
        }

        var result = await userService.GetUsersAsync(parsedRole, search, cancellationToken);
        return ToActionResult(result);
    }

    [HttpGet("teachers")]
    public async Task<IActionResult> GetTeachers(
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        var result = await userService.GetTeachersAsync(search, cancellationToken);
        return ToActionResult(result);
    }

    [HttpGet("students")]
    public async Task<IActionResult> GetStudents(
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        var result = await userService.GetStudentsAsync(search, cancellationToken);
        return ToActionResult(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await userService.GetUserByIdAsync(id, GetCurrentUserId(), isAdmin: true, cancellationToken);
        return ToActionResult(result);
    }

    [HttpPut("teacher/{id:int}")]
    public async Task<IActionResult> UpdateTeacher(
        int id,
        [FromBody] UpdateTeacherDto dto,
        CancellationToken cancellationToken)
    {
        var result = await userService.UpdateTeacherAsync(id, dto, cancellationToken);
        return ToActionResult(result);
    }

    [HttpPut("student/{id:int}")]
    public async Task<IActionResult> UpdateStudent(
        int id,
        [FromBody] UpdateStudentDto dto,
        CancellationToken cancellationToken)
    {
        var result = await userService.UpdateStudentAsync(id, dto, cancellationToken);
        return ToActionResult(result);
    }

    [HttpPut("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(
        int id,
        [FromBody] UpdateUserStatusDto dto,
        CancellationToken cancellationToken)
    {
        var currentAdminId = GetCurrentUserId() ?? 0;
        var result = await userService.UpdateStatusAsync(id, dto, currentAdminId, cancellationToken);
        return ToActionResult(result);
    }

    [HttpPost("{id:int}/reset-password")]
    public async Task<IActionResult> ResetPassword(
        int id,
        [FromBody] ResetPasswordDto dto,
        CancellationToken cancellationToken)
    {
        var result = await userService.ResetPasswordAsync(id, dto, cancellationToken);
        if (!result.Succeeded)
        {
            return ToActionResult(result);
        }

        return Ok(new { message = "Password has been reset successfully." });
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

