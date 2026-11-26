using ELearning.Domain.Entities;

namespace ELearning.Application.Interfaces.Repositories;

public interface ICourseRepository
{
    Task<IReadOnlyList<Course>> GetPublishedCoursesAsync(
        string? search,
        CancellationToken cancellationToken = default);

    Task<Course?> GetPublishedCourseAsync(
        int id,
        CancellationToken cancellationToken = default);
}
