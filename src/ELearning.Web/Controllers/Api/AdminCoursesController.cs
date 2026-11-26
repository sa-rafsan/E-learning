using ELearning.Application.Common;
using ELearning.Application.DTOs;
using ELearning.Application.Interfaces.Services;
using ELearning.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ELearning.Web.Controllers.Api;

[ApiController]
[Authorize(Roles = nameof(UserRole.Admin))]
[Route("api/admin/courses")]
public sealed class AdminCoursesController(IAdminCourseService courseService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetCourses(CancellationToken cancellationToken)
    {
        var result = await courseService.GetCoursesAsync(cancellationToken);
        return ToActionResult(result);
    }

    [HttpGet("options")]
    public async Task<IActionResult> GetAssignmentOptions(CancellationToken cancellationToken)
    {
        var teachers = await courseService.GetTeacherOptionsAsync(cancellationToken);
        var students = await courseService.GetStudentOptionsAsync(cancellationToken);

        return Ok(new
        {
            teachers = teachers.Select(t => new
            {
                t.Id,
                t.UserId,
                t.FullName,
                t.Email,
                t.Department
            }),
            students = students.Select(s => new
            {
                s.Id,
                s.UserId,
                s.StudentId,
                s.FullName,
                s.Email,
                s.Batch,
                s.Section
            })
        });
    }

    [HttpPost]
    public async Task<IActionResult> CreateCourse(
        [FromBody] CreateCourseDto dto,
        CancellationToken cancellationToken)
    {
        var result = await courseService.CreateCourseAsync(dto, cancellationToken);
        return ToActionResult(result);
    }

    [HttpPut("{id:int}/teacher")]
    public async Task<IActionResult> AssignTeacher(
        int id,
        [FromBody] AssignCourseTeacherDto dto,
        CancellationToken cancellationToken)
    {
        var result = await courseService.AssignTeacherAsync(id, dto, cancellationToken);
        return ToActionResult(result);
    }

    [HttpPost("{id:int}/students")]
    public async Task<IActionResult> AssignStudents(
        int id,
        [FromBody] AssignCourseStudentsDto dto,
        CancellationToken cancellationToken)
    {
        var result = await courseService.AssignStudentsAsync(id, dto, cancellationToken);
        return ToActionResult(result);
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

