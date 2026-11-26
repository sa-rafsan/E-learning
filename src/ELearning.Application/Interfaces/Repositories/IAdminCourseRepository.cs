using ELearning.Domain.Entities;

namespace ELearning.Application.Interfaces.Repositories;

public interface IAdminCourseRepository
{
    Task<IReadOnlyList<Course>> GetCoursesWithDetailsAsync(CancellationToken cancellationToken = default);

    Task<Course?> GetCourseWithEnrollmentsAsync(int courseId, CancellationToken cancellationToken = default);

    Task<Teacher?> GetTeacherByIdAsync(int teacherId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Teacher>> GetAllTeachersAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Student>> GetAllStudentsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Student>> GetStudentsByIdsAsync(
        IEnumerable<int> studentIds,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Student>> GetStudentsByBatchAndSectionAsync(
        string batch,
        string? section,
        CancellationToken cancellationToken = default);

    Task AddCourseAsync(Course course, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

