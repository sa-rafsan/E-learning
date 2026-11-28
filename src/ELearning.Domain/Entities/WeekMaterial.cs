namespace ELearning.Domain.Entities;

public class WeekMaterial
{
    public int Id { get; set; }

    public int CourseWeekId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string MaterialType { get; set; } = "Link";

    public string? FilePath { get; set; }

    public string? ExternalUrl { get; set; }

    public DateTime UploadedAtUtc { get; set; } = DateTime.UtcNow;

    public CourseWeek? Week { get; set; }
}

