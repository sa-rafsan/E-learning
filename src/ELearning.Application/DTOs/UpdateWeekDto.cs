namespace ELearning.Application.DTOs;

public sealed class UpdateWeekDto
{
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? LearningObjectives { get; set; }

    public int? AssignedTeacherId { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }
}

