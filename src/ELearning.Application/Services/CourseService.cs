using ELearning.Application.DTOs;
using ELearning.Application.Interfaces.Repositories;
using ELearning.Application.Interfaces.Services;

namespace ELearning.Application.Services;

public sealed class CourseService(ICourseRepository courseRepository) : ICourseService
{
    public async Task<IReadOnlyList<CourseSummaryDto>> GetPublishedCoursesAsync(
        string? search,
        CancellationToken cancellationToken = default)
    {
        var courses = await courseRepository.GetPublishedCoursesAsync(search, cancellationToken);

        return courses
            .Select(course => new CourseSummaryDto(
                course.Id,
                course.Title,
                course.Description,
                course.Instructor,
                course.Category,
                course.DurationHours,
                course.Lessons.Count))
            .ToList();
    }

    public async Task<CourseDetailDto?> GetPublishedCourseAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var course = await courseRepository.GetPublishedCourseAsync(id, cancellationToken);
        if (course is null)
        {
            return null;
        }

        var lessons = course.Lessons
            .OrderBy(lesson => lesson.Order)
            .Select(lesson => new LessonSummaryDto(
                lesson.Title,
                lesson.Summary,
                lesson.DurationMinutes,
                lesson.Order))
            .ToList();

        return new CourseDetailDto(
            course.Id,
            course.Title,
            course.Description,
            course.Instructor,
            course.Category,
            course.DurationHours,
            lessons);
    }
}
