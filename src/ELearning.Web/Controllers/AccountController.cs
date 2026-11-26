using System.Security.Claims;
using ELearning.Application.Interfaces.Services;
using ELearning.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ELearning.Web.Controllers;

public sealed class AccountController(IAuthService authService) : Controller
{
    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            if (User.IsInRole(nameof(UserRole.Admin)))
            {
                return Redirect("/Admin/Dashboard");
            }

            if (User.IsInRole(nameof(UserRole.Teacher)))
            {
                return Redirect("/Teacher/Dashboard");
            }

            if (User.IsInRole(nameof(UserRole.Student)))
            {
                return Redirect("/Student/Dashboard");
            }
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        int? userId = null;
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(userIdClaim, out var parsedId))
        {
            userId = parsedId;
        }

        var refreshToken = Request.Cookies["ELearning.RefreshToken"];
        await authService.LogoutAsync(userId, refreshToken, cancellationToken);

        Response.Cookies.Delete("ELearning.AccessToken", new CookieOptions { Path = "/" });
        Response.Cookies.Delete("ELearning.RefreshToken", new CookieOptions { Path = "/" });

        return RedirectToAction(nameof(Login));
    }
}

