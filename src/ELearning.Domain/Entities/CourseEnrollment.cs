namespace ELearning.Domain.Entities;

public class CourseEnrollment
{
    public int Id { get; set; }

    public int CourseId { get; set; }

    public int? CourseBatchId { get; set; }

    public int StudentId { get; set; }

    public DateTime EnrolledAtUtc { get; set; } = DateTime.UtcNow;

    public Course? Course { get; set; }

    public CourseBatch? Batch { get; set; }

    public Student? Student { get; set; }
}

