using ELearning.Application.Common;
using ELearning.Application.DTOs;
using ELearning.Domain.Entities;

namespace ELearning.Application.Interfaces.Services;

public interface IAdminCourseService
{
    Task<ServiceResult<IReadOnlyList<AdminCourseDto>>> GetCoursesAsync(CancellationToken cancellationToken = default);

    Task<ServiceResult<AdminCourseDto>> GetCourseByIdAsync(
        int courseId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<AdminCourseDto>> CreateCourseAsync(
        CreateCourseDto dto,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<AdminCourseDto>> UpdateCourseAsync(
        int courseId,
        CreateCourseDto dto,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<AdminCourseDto>> AssignTeacherAsync(
        int courseId,
        AssignCourseTeacherDto dto,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<AdminCourseDto>> AssignStudentsAsync(
        int courseId,
        AssignCourseStudentsDto dto,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Teacher>> GetTeacherOptionsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Student>> GetStudentOptionsAsync(CancellationToken cancellationToken = default);
}

