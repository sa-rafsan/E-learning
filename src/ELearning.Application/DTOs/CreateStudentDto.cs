using ELearning.Domain.Enums;

namespace ELearning.Application.DTOs;

public sealed class CreateStudentDto
{
    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public string StudentId { get; set; } = string.Empty;

    public string? Department { get; set; }

    public string Password { get; set; } = string.Empty;

    public string ConfirmPassword { get; set; } = string.Empty;

    public UserStatus Status { get; set; } = UserStatus.Active;

    public string? ImagePath { get; set; }
}

