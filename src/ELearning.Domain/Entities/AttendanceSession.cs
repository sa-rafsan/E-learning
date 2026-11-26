namespace ELearning.Domain.Entities;

public class AttendanceSession
{
    public int Id { get; set; }

    public int CourseId { get; set; }

    public int TeacherId { get; set; }

    public DateOnly SessionDate { get; set; }

    public string Batch { get; set; } = string.Empty;

    public string Section { get; set; } = string.Empty;

    public string? Topic { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAtUtc { get; set; }

    public Course? Course { get; set; }

    public Teacher? Teacher { get; set; }

    public ICollection<AttendanceRecord> Records { get; set; } = new List<AttendanceRecord>();
}

