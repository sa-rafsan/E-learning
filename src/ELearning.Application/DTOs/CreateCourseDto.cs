namespace ELearning.Application.DTOs;

public sealed class CreateCourseDto
{
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public int DurationHours { get; set; } = 1;

    public bool IsPublished { get; set; } = true;

    public int? TeacherId { get; set; }

    public string? ImagePath { get; set; }
}

