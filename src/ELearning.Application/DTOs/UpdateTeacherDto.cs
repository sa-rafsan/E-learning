using ELearning.Domain.Enums;

namespace ELearning.Application.DTOs;

public sealed class UpdateTeacherDto
{
    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public UserStatus Status { get; set; } = UserStatus.Active;

    public string? ImagePath { get; set; }
}

