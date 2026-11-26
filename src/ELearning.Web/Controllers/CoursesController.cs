using ELearning.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace ELearning.Web.Controllers;

public class CoursesController(ICourseService courseService) : Controller
{
    public async Task<IActionResult> Index(string? search, CancellationToken cancellationToken)
    {
        ViewData["SearchTerm"] = search;
        var courses = await courseService.GetPublishedCoursesAsync(search, cancellationToken);
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
