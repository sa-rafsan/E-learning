using ELearning.Domain.Enums;

namespace ELearning.Domain.Entities;

public class WeekActivity
{
    public int Id { get; set; }

    public int CourseWeekId { get; set; }

    public int? CourseBatchId { get; set; }

    public int TeacherId { get; set; }

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

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public CourseWeek? Week { get; set; }

    public CourseBatch? Batch { get; set; }

    public Teacher? Teacher { get; set; }

    public ICollection<ActivityQuestion> Questions { get; set; } = new List<ActivityQuestion>();

    public ICollection<ActivitySubmission> Submissions { get; set; } = new List<ActivitySubmission>();

    public ICollection<ForumPost> ForumPosts { get; set; } = new List<ForumPost>();
}

