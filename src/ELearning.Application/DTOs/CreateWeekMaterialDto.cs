namespace ELearning.Application.DTOs;

public sealed class CreateWeekMaterialDto
{
    public string Title { get; set; } = string.Empty;

    public string MaterialType { get; set; } = "Link";

    public string? FilePath { get; set; }

    public string? ExternalUrl { get; set; }
}

