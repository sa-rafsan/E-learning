namespace ELearning.Domain.Entities;

public class LessonProgress
{
    public int Id { get; set; }

    public int StudentId { get; set; }

    public int LessonId { get; set; }

    public DateTime CompletedAtUtc { get; set; } = DateTime.UtcNow;

    public Student? Student { get; set; }

    public Lesson? Lesson { get; set; }
}

