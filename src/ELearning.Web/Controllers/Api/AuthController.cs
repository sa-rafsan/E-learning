using System.Security.Claims;
using ELearning.Application.Common;
using ELearning.Application.DTOs;
using ELearning.Application.Interfaces.Services;
using ELearning.Application.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ELearning.Web.Controllers.Api;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    IAuthService authService,
    IWebHostEnvironment environment,
    IOptions<AdminSettings> adminOptions) : ControllerBase
{
    private const string AccessTokenCookieName = "ELearning.AccessToken";
    private const string RefreshTokenCookieName = "ELearning.RefreshToken";

    [AllowAnonymous]
    [HttpGet("demo-credentials")]
    public IActionResult GetDemoCredentials([FromQuery] string role)
    {
        if (!environment.IsDevelopment())
        {
            return NotFound();
        }

        return role?.Trim().ToLowerInvariant() switch
        {
            "admin" => Ok(new
            {
                email = adminOptions.Value.Email,
                password = adminOptions.Value.Password
            }),
            "teacher" => Ok(new
            {
                email = "ayesha.rahman@wub.edu.bd",
                password = "Teacher@123"
            }),
            "student" => Ok(new
            {
                email = "0325704650@student.wub.edu.bd",
                password = "0325704650"
            }),
            _ => BadRequest(new { message = "Invalid role." })
        };
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto, CancellationToken cancellationToken)
    {
        var result = await authService.LoginAsync(dto, cancellationToken);
        if (!result.Succeeded || result.Data is null)
        {
            return ToErrorResult(result);
        }

        SetAuthCookies(result.Data, dto.RememberMe);
        return Ok(result.Data);
    }

    [AllowAnonymous]
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken(
        [FromBody] RefreshTokenDto? dto,
        CancellationToken cancellationToken)
    {
        var token = !string.IsNullOrWhiteSpace(dto?.RefreshToken)
            ? dto.RefreshToken
            : Request.Cookies[RefreshTokenCookieName] ?? string.Empty;

        var result = await authService.RefreshTokenAsync(token, cancellationToken);
        if (!result.Succeeded || result.Data is null)
        {
            ClearAuthCookies();
            return ToErrorResult(result);
        }

        SetAuthCookies(result.Data, rememberMe: true);
        return Ok(result.Data);
    }

    [AllowAnonymous]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(
        [FromBody] LogoutDto? dto,
        CancellationToken cancellationToken)
    {
        int? userId = null;
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(userIdClaim, out var parsedId))
        {
            userId = parsedId;
        }

        var refreshToken = !string.IsNullOrWhiteSpace(dto?.RefreshToken)
            ? dto.RefreshToken
            : Request.Cookies[RefreshTokenCookieName];

        await authService.LogoutAsync(userId, refreshToken, cancellationToken);
        ClearAuthCookies();

        return Ok(new { message = "Logged out successfully." });
    }

    private void SetAuthCookies(LoginResponseDto response, bool rememberMe)
    {
        var isHttps = Request.IsHttps;

        Response.Cookies.Append(AccessTokenCookieName, response.AccessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = isHttps,
            SameSite = SameSiteMode.Lax,
            Expires = response.ExpiresAtUtc,
            Path = "/"
        });

        Response.Cookies.Append(RefreshTokenCookieName, response.RefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = isHttps,
            SameSite = SameSiteMode.Lax,
            Expires = rememberMe ? DateTimeOffset.UtcNow.AddDays(14) : DateTimeOffset.UtcNow.AddDays(1),
            Path = "/"
        });
    }

    private void ClearAuthCookies()
    {
        Response.Cookies.Delete(AccessTokenCookieName, new CookieOptions { Path = "/" });
        Response.Cookies.Delete(RefreshTokenCookieName, new CookieOptions { Path = "/" });
    }

    private IActionResult ToErrorResult<T>(ServiceResult<T> result)
    {
        return StatusCode(result.StatusCode, new
        {
            message = result.ErrorMessage,
            errors = result.ValidationErrors
        });
    }
}

