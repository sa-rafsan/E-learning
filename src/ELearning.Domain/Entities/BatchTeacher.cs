namespace ELearning.Domain.Entities;

public class BatchTeacher
{
    public int Id { get; set; }

    public int CourseBatchId { get; set; }

    public int TeacherId { get; set; }

    public DateTime AssignedAtUtc { get; set; } = DateTime.UtcNow;

    public CourseBatch? Batch { get; set; }

    public Teacher? Teacher { get; set; }
}

