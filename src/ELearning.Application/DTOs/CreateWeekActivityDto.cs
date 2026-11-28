using ELearning.Domain.Enums;

namespace ELearning.Application.DTOs;

public sealed class CreateWeekActivityDto
{
    public int? CourseBatchId { get; set; }

    public ActivityType Type { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Instructions { get; set; }

    public string? RequiredTasks { get; set; }

    public string? AttachmentPath { get; set; }

    public decimal MaxMarks { get; set; } = 100;

    public decimal? PassingMarks { get; set; }

    public DateTime StartAtUtc { get; set; }

    public DateTime EndAtUtc { get; set; }

    public DateTime? SubmissionDeadlineUtc { get; set; }

    public int? TimeLimitMinutes { get; set; }

    public bool AllowLateSubmission { get; set; }

    public ActivityStatus Status { get; set; } = ActivityStatus.Published;

    public List<ActivityQuestionInputDto> Questions { get; set; } = [];
}

