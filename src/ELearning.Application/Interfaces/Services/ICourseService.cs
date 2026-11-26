using ELearning.Application.DTOs;

namespace ELearning.Application.Interfaces.Services;

public interface ICourseService
{
    Task<IReadOnlyList<CourseSummaryDto>> GetPublishedCoursesAsync(
        string? search,
        CancellationToken cancellationToken = default);

    Task<CourseDetailDto?> GetPublishedCourseAsync(
        int id,
        CancellationToken cancellationToken = default);
}
