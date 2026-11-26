namespace ELearning.Application.Options;

public sealed class AdminSettings
{
    public const string SectionName = "AdminSettings";

    public string FullName { get; set; } = "Platform Administrator";

    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}

