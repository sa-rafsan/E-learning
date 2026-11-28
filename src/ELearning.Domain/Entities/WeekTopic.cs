namespace ELearning.Domain.Entities;

public class WeekTopic
{
    public int Id { get; set; }

    public int CourseWeekId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int Order { get; set; }

    public CourseWeek? Week { get; set; }
}

