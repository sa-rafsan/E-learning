namespace ELearning.Application.DTOs;

public sealed class LoginResponseDto
{
    public string AccessToken { get; set; } = string.Empty;

    public string RefreshToken { get; set; } = string.Empty;

    public DateTime ExpiresAtUtc { get; set; }

    public string RedirectUrl { get; set; } = string.Empty;

    public UserResponseDto User { get; set; } = null!;
}

