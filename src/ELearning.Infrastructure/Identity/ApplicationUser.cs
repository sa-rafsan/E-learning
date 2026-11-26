using ELearning.Domain.Entities;
using ELearning.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace ELearning.Infrastructure.Identity;

public class ApplicationUser : IdentityUser<int>
{
    public string FullName { get; set; } = string.Empty;

    public UserRole Role { get; set; }

    public UserStatus Status { get; set; } = UserStatus.Active;

    public string? StudentId { get; set; }

    public string? Department { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAtUtc { get; set; }

    public Teacher? TeacherProfile { get; set; }

    public Student? StudentProfile { get; set; }

    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
