using System.Security.Claims;
using ELearning.Application.Interfaces.Services;
using ELearning.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ELearning.Web.Controllers;

public class CoursesController(ICourseService courseService, ApplicationDbContext db) : Controller
{
    public async Task<IActionResult> Index(string? search, CancellationToken cancellationToken)
    {
        ViewData["SearchTerm"] = search;
        var courses = await courseService.GetPublishedCoursesAsync(search, cancellationToken);

        if (User.IsInRole("Student"))
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            var email = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue("email");
            int.TryParse(userIdStr, out var userId);

            var student = await db.Students.AsNoTracking()
                .FirstOrDefaultAsync(s => (userId > 0 && s.UserId == userId) || (!string.IsNullOrEmpty(email) && s.Email == email), cancellationToken);

            if (student is not null)
            {
                var enrolledIds = await db.CourseEnrollments.AsNoTracking()
                    .Where(e => e.StudentId == student.Id)
                    .Select(e => e.CourseId)
                    .Distinct()
                    .ToListAsync(cancellationToken);

                courses = courses.Where(c => enrolledIds.Contains(c.Id)).ToList();
                var courseIds = courses.Select(c => c.Id).ToList();
                ViewData["CourseImages"] = await db.Courses.AsNoTracking()
                    .Where(c => courseIds.Contains(c.Id))
                    .ToDictionaryAsync(c => c.Id, c => c.ImagePath, cancellationToken);
            }
            else
            {
                courses = [];
            }
        }

        return View(courses);
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var course = await courseService.GetPublishedCourseAsync(id, cancellationToken);
        if (course is null)
        {
            return NotFound();
        }

        return View(course);
    }
}
