using ELearning.Application.Interfaces.Repositories;
using ELearning.Domain.Entities;
using ELearning.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ELearning.Infrastructure.Repositories;

public sealed class AttendanceRepository(ApplicationDbContext dbContext) : IAttendanceRepository
{
    public Task<Teacher?> GetTeacherByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return dbContext.Teachers
            .AsNoTracking()
            .SingleOrDefaultAsync(t => t.UserId == userId, cancellationToken);
    }

    public Task<Student?> GetStudentByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return dbContext.Students
            .AsNoTracking()
            .SingleOrDefaultAsync(s => s.UserId == userId, cancellationToken);
    }

    public Task<Course?> GetCourseByIdAsync(int courseId, CancellationToken cancellationToken = default)
    {
        return dbContext.Courses
            .AsNoTracking()
            .SingleOrDefaultAsync(c => c.Id == courseId, cancellationToken);
    }

    public async Task<IReadOnlyList<Student>> GetStudentsByBatchAndSectionAsync(
        string batch,
        string section,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Students
            .AsNoTracking()
            .Where(s => s.Batch == batch && s.Section == section)
            .OrderBy(s => s.StudentId)
            .ToListAsync(cancellationToken);
    }

    public Task<AttendanceSession?> GetSessionAsync(
        int courseId,
        DateOnly sessionDate,
        string batch,
        string section,
        CancellationToken cancellationToken = default)
    {
        return dbContext.AttendanceSessions
            .Include(s => s.Teacher)
            .Include(s => s.Records)
            .SingleOrDefaultAsync(
                s => s.CourseId == courseId &&
                     s.SessionDate == sessionDate &&
                     s.Batch == batch &&
                     s.Section == section,
                cancellationToken);
    }

    public async Task<IReadOnlyList<AttendanceSession>> GetTeacherSessionsAsync(
        int teacherId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.AttendanceSessions
            .AsNoTracking()
            .Include(s => s.Course)
            .Include(s => s.Teacher)
            .Include(s => s.Records)
            .Where(s => s.TeacherId == teacherId)
            .OrderByDescending(s => s.SessionDate)
            .ThenByDescending(s => s.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AttendanceRecord>> GetStudentRecordsAsync(
        int studentId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.AttendanceRecords
            .AsNoTracking()
            .Include(r => r.Session)
                .ThenInclude(s => s!.Course)
            .Include(r => r.Session)
                .ThenInclude(s => s!.Teacher)
            .Where(r => r.StudentId == studentId)
            .OrderByDescending(r => r.Session!.SessionDate)
            .ThenByDescending(r => r.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task AddSessionAsync(AttendanceSession session, CancellationToken cancellationToken = default)
    {
        await dbContext.AttendanceSessions.AddAsync(session, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}

