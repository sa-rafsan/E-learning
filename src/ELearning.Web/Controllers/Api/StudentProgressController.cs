using System.Security.Claims;
using ELearning.Domain.Entities;
using ELearning.Domain.Enums;
using ELearning.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ELearning.Web.Controllers.Api;

[ApiController]
[Authorize(Roles = nameof(UserRole.Student))]
[Route("api/student/progress")]
public sealed class StudentProgressController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetCourseProgress(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var student = await dbContext.Students
            .AsNoTracking()
            .SingleOrDefaultAsync(s => s.UserId == userId.Value, cancellationToken);

        if (student is null)
        {
            return NotFound(new { message = "Student profile not found." });
        }

        var enrolledCourseIds = await dbContext.CourseEnrollments
            .AsNoTracking()
            .Where(e => e.StudentId == student.Id)
            .Select(e => e.CourseId)
            .ToListAsync(cancellationToken);

        var coursesQuery = dbContext.Courses
            .AsNoTracking()
            .Include(c => c.Lessons)
            .Where(c => c.IsPublished);

        if (enrolledCourseIds.Count > 0)
        {
            coursesQuery = coursesQuery.Where(c => enrolledCourseIds.Contains(c.Id));
        }

        var courses = await coursesQuery
            .OrderBy(c => c.Title)
            .ToListAsync(cancellationToken);

        var completedLessonIds = await dbContext.LessonProgresses
            .AsNoTracking()
            .Where(lp => lp.StudentId == student.Id)
            .Select(lp => lp.LessonId)
            .ToListAsync(cancellationToken);

        var completedSet = completedLessonIds.ToHashSet();

        var result = courses.Select(c =>
        {
            var lessons = c.Lessons.OrderBy(l => l.Order).ToList();
            var total = lessons.Count;
            var completed = lessons.Count(l => completedSet.Contains(l.Id));
            var percentage = total > 0 ? (int)Math.Round(completed * 100.0 / total) : 0;

            return new
            {
                courseId = c.Id,
                title = c.Title,
                category = c.Category,
                totalLessons = total,
                completedLessons = completed,
                remainingLessons = Math.Max(0, total - completed),
                percentage,
                lessons = lessons.Select(l => new
                {
                    lessonId = l.Id,
                    title = l.Title,
                    durationMinutes = l.DurationMinutes,
                    order = l.Order,
                    isCompleted = completedSet.Contains(l.Id)
                })
            };
        });

        return Ok(result);
    }

    [HttpPost("lessons/{lessonId:int}/toggle")]
    public async Task<IActionResult> ToggleLessonCompletion(
        int lessonId,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var student = await dbContext.Students
            .AsNoTracking()
            .SingleOrDefaultAsync(s => s.UserId == userId.Value, cancellationToken);

        if (student is null)
        {
            return NotFound(new { message = "Student profile not found." });
        }

        var lessonExists = await dbContext.Lessons
            .AsNoTracking()
            .AnyAsync(l => l.Id == lessonId, cancellationToken);

        if (!lessonExists)
        {
            return NotFound(new { message = "Lesson not found." });
        }

        var existing = await dbContext.LessonProgresses
            .SingleOrDefaultAsync(
                lp => lp.StudentId == student.Id && lp.LessonId == lessonId,
                cancellationToken);

        if (existing is not null)
        {
            dbContext.LessonProgresses.Remove(existing);
        }
        else
        {
            await dbContext.LessonProgresses.AddAsync(new LessonProgress
            {
                StudentId = student.Id,
                LessonId = lessonId,
                CompletedAtUtc = DateTime.UtcNow
            }, cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(new { lessonId, isCompleted = existing is null });
    }

    private int? GetCurrentUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(claim, out var id) ? id : null;
    }
}

