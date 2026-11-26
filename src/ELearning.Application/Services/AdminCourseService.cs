using ELearning.Application.Common;
using ELearning.Application.DTOs;
using ELearning.Application.Interfaces.Repositories;
using ELearning.Application.Interfaces.Services;
using ELearning.Domain.Entities;
using FluentValidation;

namespace ELearning.Application.Services;

public sealed class AdminCourseService(
    IAdminCourseRepository courseRepository,
    IValidator<CreateCourseDto> createValidator) : IAdminCourseService
{
    public async Task<ServiceResult<IReadOnlyList<AdminCourseDto>>> GetCoursesAsync(
        CancellationToken cancellationToken = default)
    {
        var courses = await courseRepository.GetCoursesWithDetailsAsync(cancellationToken);
        return ServiceResult<IReadOnlyList<AdminCourseDto>>.Ok(courses.Select(MapToDto).ToList());
    }

    public async Task<ServiceResult<AdminCourseDto>> CreateCourseAsync(
        CreateCourseDto dto,
        CancellationToken cancellationToken = default)
    {
        var validation = await createValidator.ValidateAsync(dto, cancellationToken);
        if (!validation.IsValid)
        {
            var errors = validation.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

            return ServiceResult<AdminCourseDto>.Fail("Validation failed.", 400, errors);
        }

        Teacher? teacher = null;
        if (dto.TeacherId.HasValue && dto.TeacherId.Value > 0)
        {
            teacher = await courseRepository.GetTeacherByIdAsync(dto.TeacherId.Value, cancellationToken);
            if (teacher is null)
            {
                return ServiceResult<AdminCourseDto>.Fail("Selected teacher not found.", 404);
            }
        }

        var course = new Course
        {
            Title = dto.Title.Trim(),
            Description = dto.Description.Trim(),
            Category = dto.Category.Trim(),
            DurationHours = dto.DurationHours,
            IsPublished = dto.IsPublished,
            TeacherId = teacher?.Id,
            Instructor = teacher?.FullName ?? "TBA",
            CreatedAtUtc = DateTime.UtcNow
        };

        await courseRepository.AddCourseAsync(course, cancellationToken);
        await courseRepository.SaveChangesAsync(cancellationToken);

        course.Teacher = teacher;
        return ServiceResult<AdminCourseDto>.Ok(MapToDto(course), 201);
    }

    public async Task<ServiceResult<AdminCourseDto>> AssignTeacherAsync(
        int courseId,
        AssignCourseTeacherDto dto,
        CancellationToken cancellationToken = default)
    {
        if (dto.TeacherId <= 0)
        {
            return ServiceResult<AdminCourseDto>.Fail("Please select a valid teacher.", 400);
        }

        var course = await courseRepository.GetCourseWithEnrollmentsAsync(courseId, cancellationToken);
        if (course is null)
        {
            return ServiceResult<AdminCourseDto>.Fail("Course not found.", 404);
        }

        var teacher = await courseRepository.GetTeacherByIdAsync(dto.TeacherId, cancellationToken);
        if (teacher is null)
        {
            return ServiceResult<AdminCourseDto>.Fail("Teacher not found.", 404);
        }

        course.TeacherId = teacher.Id;
        course.Instructor = teacher.FullName;
        await courseRepository.SaveChangesAsync(cancellationToken);

        course.Teacher = teacher;
        return ServiceResult<AdminCourseDto>.Ok(MapToDto(course));
    }

    public async Task<ServiceResult<AdminCourseDto>> AssignStudentsAsync(
        int courseId,
        AssignCourseStudentsDto dto,
        CancellationToken cancellationToken = default)
    {
        var course = await courseRepository.GetCourseWithEnrollmentsAsync(courseId, cancellationToken);
        if (course is null)
        {
            return ServiceResult<AdminCourseDto>.Fail("Course not found.", 404);
        }

        var targetStudents = new List<Student>();

        if (!string.IsNullOrWhiteSpace(dto.Batch))
        {
            var batchStudents = await courseRepository.GetStudentsByBatchAndSectionAsync(
                dto.Batch.Trim(),
                string.IsNullOrWhiteSpace(dto.Section) ? null : dto.Section.Trim().ToUpperInvariant(),
                cancellationToken);
            targetStudents.AddRange(batchStudents);
        }

        if (dto.StudentIds is { Count: > 0 })
        {
            var selected = await courseRepository.GetStudentsByIdsAsync(dto.StudentIds, cancellationToken);
            targetStudents.AddRange(selected);
        }

        var distinctStudents = targetStudents
            .GroupBy(s => s.Id)
            .Select(g => g.First())
            .ToList();

        if (distinctStudents.Count == 0)
        {
            return ServiceResult<AdminCourseDto>.Fail(
                "Please select at least one student or a valid batch/section.",
                400);
        }

        var existingIds = course.Enrollments.Select(e => e.StudentId).ToHashSet();
        var now = DateTime.UtcNow;

        foreach (var student in distinctStudents)
        {
            if (!existingIds.Contains(student.Id))
            {
                course.Enrollments.Add(new CourseEnrollment
                {
                    CourseId = course.Id,
                    StudentId = student.Id,
                    EnrolledAtUtc = now
                });
            }
        }

        await courseRepository.SaveChangesAsync(cancellationToken);

        return ServiceResult<AdminCourseDto>.Ok(MapToDto(course));
    }

    public Task<IReadOnlyList<Teacher>> GetTeacherOptionsAsync(CancellationToken cancellationToken = default)
    {
        return courseRepository.GetAllTeachersAsync(cancellationToken);
    }

    public Task<IReadOnlyList<Student>> GetStudentOptionsAsync(CancellationToken cancellationToken = default)
    {
        return courseRepository.GetAllStudentsAsync(cancellationToken);
    }

    private static AdminCourseDto MapToDto(Course course) => new()
    {
        Id = course.Id,
        Title = course.Title,
        Description = course.Description,
        Category = course.Category,
        DurationHours = course.DurationHours,
        IsPublished = course.IsPublished,
        TeacherId = course.TeacherId,
        TeacherName = course.Teacher?.FullName ?? course.Instructor,
        EnrolledStudentsCount = course.Enrollments.Count,
        EnrolledStudentIds = course.Enrollments.Select(e => e.StudentId).ToList()
    };
}

