namespace ELearning.Domain.Entities;

public class CourseWeek
{
    public int Id { get; set; }

    public int CourseId { get; set; }

    public int WeekNumber { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? LearningObjectives { get; set; }

    public int? AssignedTeacherId { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public Course? Course { get; set; }

    public Teacher? AssignedTeacher { get; set; }

    public ICollection<WeekTopic> Topics { get; set; } = new List<WeekTopic>();

    public ICollection<WeekMaterial> Materials { get; set; } = new List<WeekMaterial>();

    public ICollection<WeekActivity> Activities { get; set; } = new List<WeekActivity>();
}

