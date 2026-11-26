namespace ELearning.Application.DTOs;

public sealed class ResetPasswordDto
{
    public string NewPassword { get; set; } = string.Empty;

    public string ConfirmPassword { get; set; } = string.Empty;
}

