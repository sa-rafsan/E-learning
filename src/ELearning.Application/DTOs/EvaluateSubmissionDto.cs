namespace ELearning.Application.DTOs;

public sealed class EvaluateSubmissionDto
{
    public decimal ObtainedMarks { get; set; }

    public string? Feedback { get; set; }

    public bool IsPublished { get; set; } = true;
}

