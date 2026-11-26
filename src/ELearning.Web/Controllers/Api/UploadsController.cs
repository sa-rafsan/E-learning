using ELearning.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ELearning.Web.Controllers.Api;

[ApiController]
[Authorize(Roles = nameof(UserRole.Admin))]
[Route("api/admin/uploads")]
public sealed class UploadsController(IWebHostEnvironment environment) : ControllerBase
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp"
    };

    private static readonly HashSet<string> AllowedFolders = new(StringComparer.OrdinalIgnoreCase)
    {
        "students", "teachers", "courses"
    };

    private const long MaxFileSizeBytes = 5 * 1024 * 1024;

    [HttpPost("{folder}")]
    public async Task<IActionResult> UploadImage(
        string folder,
        IFormFile? file,
        CancellationToken cancellationToken)
    {
        if (!AllowedFolders.Contains(folder))
        {
            return BadRequest(new { message = "Invalid upload category." });
        }

        if (file is null || file.Length == 0)
        {
            return BadRequest(new { message = "Please select an image file to upload." });
        }

        if (file.Length > MaxFileSizeBytes)
        {
            return BadRequest(new { message = "Image size cannot exceed 5 MB." });
        }

        var ext = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(ext) || !AllowedExtensions.Contains(ext))
        {
            return BadRequest(new { message = "Only .jpg, .jpeg, .png, and .webp images are allowed." });
        }

        var safeFolder = folder.ToLowerInvariant();
        var webRoot = environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot");
        var uploadDir = Path.Combine(webRoot, "uploads", safeFolder);
        Directory.CreateDirectory(uploadDir);

        var fileName = $"{Guid.NewGuid():N}{ext.ToLowerInvariant()}";
        var fullPath = Path.Combine(uploadDir, fileName);

        await using var stream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(stream, cancellationToken);

        return Ok(new { imagePath = $"/uploads/{safeFolder}/{fileName}" });
    }
}
