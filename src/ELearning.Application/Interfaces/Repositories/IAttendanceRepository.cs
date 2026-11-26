using ELearning.Domain.Entities;

namespace ELearning.Application.Interfaces.Repositories;

public interface IAttendanceRepository
{
    Task<Teacher?> GetTeacherByUserIdAsync(int userId, CancellationToken cancellationToken = default);

    Task<Student?> GetStudentByUserIdAsync(int userId, CancellationToken cancellationToken = default);

    Task<Course?> GetCourseByIdAsync(int courseId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Student>> GetStudentsByBatchAndSectionAsync(
        string batch,
        string section,
        CancellationToken cancellationToken = default);

    Task<AttendanceSession?> GetSessionAsync(
        int courseId,
        DateOnly sessionDate,
        string batch,
        string section,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AttendanceSession>> GetTeacherSessionsAsync(
        int teacherId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AttendanceRecord>> GetStudentRecordsAsync(
        int studentId,
        CancellationToken cancellationToken = default);

    Task AddSessionAsync(AttendanceSession session, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

