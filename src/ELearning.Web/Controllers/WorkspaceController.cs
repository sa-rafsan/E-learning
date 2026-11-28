using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ELearning.Web.Controllers;

[Authorize]
public sealed class WorkspaceController : Controller
{
    [HttpGet]
    public IActionResult Course(int id)
    {
        ViewData["CourseId"] = id;
        return View();
    }
}

