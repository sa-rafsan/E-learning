namespace ELearning.Domain.Entities;

public class ActivitySubmission
{
    public int Id { get; set; }

    public int WeekActivityId { get; set; }

    public int StudentId { get; set; }

    public string? SubmissionText { get; set; }

    public string? AttachmentPath { get; set; }

    public string? AnswersJson { get; set; }

    public decimal? ObtainedMarks { get; set; }

    public string? Feedback { get; set; }

    public bool IsAutoEvaluated { get; set; }

    public bool IsEvaluated { get; set; }

    public bool IsPublished { get; set; }

    public DateTime? StartedAtUtc { get; set; }

    public DateTime SubmittedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? EvaluatedAtUtc { get; set; }

    public WeekActivity? Activity { get; set; }

    public Student? Student { get; set; }
}

