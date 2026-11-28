namespace ELearning.Domain.Entities;

public class ForumPost
{
    public int Id { get; set; }

    public int WeekActivityId { get; set; }

    public int UserId { get; set; }

    public string AuthorName { get; set; } = string.Empty;

    public string AuthorRole { get; set; } = string.Empty;

    public int? ParentPostId { get; set; }

    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public WeekActivity? Activity { get; set; }

    public ForumPost? ParentPost { get; set; }
}

