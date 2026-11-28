using ELearning.Domain.Enums;

namespace ELearning.Domain.Entities;

public class CourseBatch
{
    public int Id { get; set; }

    public int CourseId { get; set; }

    public int? MasterBatchId { get; set; }
    public MasterBatch? MasterBatch { get; set; }

    public string BatchName { get; set; } = string.Empty;

    public string BatchCode { get; set; } = string.Empty;

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public string? Schedule { get; set; }

    public BatchStatus Status { get; set; } = BatchStatus.Active;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public Course? Course { get; set; }

    public ICollection<BatchTeacher> BatchTeachers { get; set; } = new List<BatchTeacher>();

    public ICollection<CourseEnrollment> Enrollments { get; set; } = new List<CourseEnrollment>();

    public ICollection<CourseBatchSection> BatchSections { get; set; } = new List<CourseBatchSection>();
}

