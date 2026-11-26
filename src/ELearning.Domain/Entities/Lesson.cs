namespace ELearning.Domain.Entities;

public class Lesson
{
    public int Id { get; set; }

    public int CourseId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public int DurationMinutes { get; set; }

    public int Order { get; set; }

    public Course? Course { get; set; }
}
