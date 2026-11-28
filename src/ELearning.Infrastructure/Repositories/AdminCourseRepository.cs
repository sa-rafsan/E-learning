using ELearning.Application.Interfaces.Repositories;
using ELearning.Domain.Entities;
using ELearning.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ELearning.Infrastructure.Repositories;

public sealed class AdminCourseRepository(ApplicationDbContext dbContext) : IAdminCourseRepository
{
    public async Task<IReadOnlyList<Course>> GetCoursesWithDetailsAsync(
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Courses
            .AsNoTracking()
            .Include(c => c.Teacher)
            .Include(c => c.Enrollments)
            .Include(c => c.Batches)
            .Include(c => c.Weeks)
            .OrderByDescending(c => c.Id)
            .ToListAsync(cancellationToken);
    }

    public Task<Course?> GetCourseWithEnrollmentsAsync(
        int courseId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Courses
            .Include(c => c.Teacher)
            .Include(c => c.Enrollments)
            .Include(c => c.Batches)
            .Include(c => c.Weeks)
            .SingleOrDefaultAsync(c => c.Id == courseId, cancellationToken);
    }

    public Task<Teacher?> GetTeacherByIdAsync(int teacherId, CancellationToken cancellationToken = default)
    {
        return dbContext.Teachers
            .AsNoTracking()
            .SingleOrDefaultAsync(t => t.Id == teacherId, cancellationToken);
    }

    public async Task<IReadOnlyList<Teacher>> GetAllTeachersAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Teachers
            .AsNoTracking()
            .OrderBy(t => t.FullName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Student>> GetAllStudentsAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Students
            .AsNoTracking()
            .OrderBy(s => s.StudentId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Student>> GetStudentsByIdsAsync(
        IEnumerable<int> studentIds,
        CancellationToken cancellationToken = default)
    {
        var ids = studentIds.Distinct().ToList();
        return await dbContext.Students
            .AsNoTracking()
            .Where(s => ids.Contains(s.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Student>> GetStudentsByBatchAndSectionAsync(
        string batch,
        string? section,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Students
            .AsNoTracking()
            .Where(s => s.Batch == batch);

        if (!string.IsNullOrWhiteSpace(section))
        {
            query = query.Where(s => s.Section == section);
        }

        return await query.OrderBy(s => s.StudentId).ToListAsync(cancellationToken);
    }

    public async Task AddCourseAsync(Course course, CancellationToken cancellationToken = default)
    {
        await dbContext.Courses.AddAsync(course, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}

