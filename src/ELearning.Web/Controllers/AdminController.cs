using ELearning.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ELearning.Web.Controllers;

[Authorize(Roles = nameof(UserRole.Admin))]
public sealed class AdminController : Controller
{
    [HttpGet]
    public IActionResult Dashboard()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Teachers()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Students()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Courses()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Settings()
    {
        return View();
    }

    [HttpGet]
    public IActionResult StudentForm(int? id = null)
    {
        ViewData["StudentUserId"] = id;
        return View();
    }

    [HttpGet]
    public IActionResult TeacherForm(int? id = null)
    {
        ViewData["TeacherUserId"] = id;
        return View();
    }

    [HttpGet]
    public IActionResult CourseForm(int? id = null)
    {
        ViewData["CourseId"] = id;
        return View();
    }
}
