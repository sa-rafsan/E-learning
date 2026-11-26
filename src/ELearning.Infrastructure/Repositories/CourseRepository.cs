using ELearning.Application.Interfaces.Repositories;
using ELearning.Domain.Entities;
using ELearning.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ELearning.Infrastructure.Repositories;

public sealed class CourseRepository(ApplicationDbContext dbContext) : ICourseRepository
{
    public async Task<IReadOnlyList<Course>> GetPublishedCoursesAsync(
        string? search,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Courses
            .AsNoTracking()
            .Where(course => course.IsPublished);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchTerm = search.Trim()
                .Replace("\\", "\\\\", StringComparison.Ordinal)
                .Replace("%", "\\%", StringComparison.Ordinal)
                .Replace("_", "\\_", StringComparison.Ordinal);
            var searchPattern = $"%{searchTerm}%";
            query = query.Where(course =>
                EF.Functions.Like(course.Title, searchPattern, "\\") ||
                EF.Functions.Like(course.Description, searchPattern, "\\") ||
                EF.Functions.Like(course.Instructor, searchPattern, "\\") ||
                EF.Functions.Like(course.Category, searchPattern, "\\"));
        }

        return await query
            .Include(course => course.Lessons)
            .OrderBy(course => course.Title)
            .ToListAsync(cancellationToken);
    }

    public Task<Course?> GetPublishedCourseAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Courses
            .AsNoTracking()
            .Where(course => course.IsPublished && course.Id == id)
            .Include(course => course.Lessons)
            .SingleOrDefaultAsync(cancellationToken);
    }
}
