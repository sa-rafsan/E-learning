using ELearning.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ELearning.Web.Controllers;

[Authorize(Roles = nameof(UserRole.Student))]
public sealed class StudentController : Controller
{
    [HttpGet]
    public IActionResult Dashboard()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Attendance()
    {
        return View();
    }
}

