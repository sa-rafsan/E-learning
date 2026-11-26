using System.Diagnostics;
using ELearning.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using ELearning.Web.Models;

namespace ELearning.Web.Controllers;

public class HomeController(ICourseService courseService) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var courses = await courseService.GetPublishedCoursesAsync(null, cancellationToken);
        return View(courses.Take(3).ToList());
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
