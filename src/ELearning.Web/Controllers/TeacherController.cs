using ELearning.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ELearning.Web.Controllers;

[Authorize(Roles = nameof(UserRole.Teacher))]
public sealed class TeacherController : Controller
{
    [HttpGet]
    public IActionResult Dashboard()
    {
        return View();
    }
}

