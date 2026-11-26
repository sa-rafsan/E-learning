using ELearning.Domain.Enums;

namespace ELearning.Application.DTOs;

public sealed class UserResponseDto
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public string Role { get; set; } = string.Empty;

    public string Status { get; set; } = UserStatus.Active.ToString();

    public string? StudentId { get; set; }

    public string? Department { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }
}

