namespace ELearning.Application.Options;

public sealed class JwtSettings
{
    public const string SectionName = "JwtSettings";

    public string Issuer { get; set; } = "ELearning.Platform";

    public string Audience { get; set; } = "ELearning.Users";

    public string SecretKey { get; set; } = string.Empty;

    public int AccessTokenExpirationMinutes { get; set; } = 30;

    public int RefreshTokenExpirationDays { get; set; } = 1;

    public int RememberMeRefreshTokenExpirationDays { get; set; } = 14;
}

