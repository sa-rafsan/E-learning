using ELearning.Domain.Enums;
using ELearning.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ELearning.Web.Controllers.Api;

[ApiController]
[Authorize(Roles = nameof(UserRole.Admin))]
[Route("api/admin/analytics")]
public sealed class AdminAnalyticsController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAnalytics(CancellationToken cancellationToken)
    {
        var users = await dbContext.Users
            .AsNoTracking()
            .Select(u => new { u.Role, u.Status })
            .ToListAsync(cancellationToken);

        var studentsBySection = await dbContext.Students
            .AsNoTracking()
            .Join(
                dbContext.Users.AsNoTracking(),
                s => s.UserId,
                u => u.Id,
                (s, u) => new { s.Section, u.Status })
            .ToListAsync(cancellationToken);

        var sections = new[] { "A", "B", "C", "D" }
            .Select(sec => new
            {
                section = $"Sec {sec}",
                active = studentsBySection.Count(x => x.Section == sec && x.Status == UserStatus.Active),
                inactive = studentsBySection.Count(x => x.Section == sec && x.Status != UserStatus.Active)
            })
            .ToList();

        var totalCourses = await dbContext.Courses.CountAsync(cancellationToken);
        var publishedCourses = await dbContext.Courses.CountAsync(c => c.IsPublished, cancellationToken);
        var assignedCourses = await dbContext.Courses.CountAsync(c => c.TeacherId != null, cancellationToken);
        var totalEnrollments = await dbContext.CourseEnrollments.CountAsync(cancellationToken);
        var totalSessions = await dbContext.AttendanceSessions.CountAsync(cancellationToken);

        var attendanceRecords = await dbContext.AttendanceRecords
            .AsNoTracking()
            .GroupBy(r => r.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var present = attendanceRecords.FirstOrDefault(x => x.Status == AttendanceStatus.Present)?.Count ?? 0;
        var absent = attendanceRecords.FirstOrDefault(x => x.Status == AttendanceStatus.Absent)?.Count ?? 0;
        var late = attendanceRecords.FirstOrDefault(x => x.Status == AttendanceStatus.Late)?.Count ?? 0;
        var excused = attendanceRecords.FirstOrDefault(x => x.Status == AttendanceStatus.Excused)?.Count ?? 0;

        var courseSeries = await dbContext.Courses
            .AsNoTracking()
            .OrderBy(c => c.Id)
            .Select(c => new
            {
                label = c.Title.Length > 14 ? c.Title.Substring(0, 14) + ".." : c.Title,
                enrolled = c.Enrollments.Count,
                hours = c.DurationHours
            })
            .Take(8)
            .ToListAsync(cancellationToken);

        return Ok(new
        {
            roles = new
            {
                students = users.Count(u => u.Role == UserRole.Student),
                teachers = users.Count(u => u.Role == UserRole.Teacher),
                admins = users.Count(u => u.Role == UserRole.Admin),
                active = users.Count(u => u.Status == UserStatus.Active),
                inactive = users.Count(u => u.Status != UserStatus.Active)
            },
            sections,
            attendance = new
            {
                present,
                absent,
                late,
                excused
            },
            pipeline = new
            {
                publishedCourses,
                assignedCourses,
                totalEnrollments,
                totalSessions,
                totalCourses
            },
            courseSeries
        });
    }
}

