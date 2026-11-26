using ELearning.Domain.Enums;

namespace ELearning.Domain.Entities;

public class AttendanceRecord
{
    public int Id { get; set; }

    public int AttendanceSessionId { get; set; }

    public int StudentId { get; set; }

    public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;

    public string? Remarks { get; set; }

    public DateTime MarkedAtUtc { get; set; } = DateTime.UtcNow;

    public AttendanceSession? Session { get; set; }

    public Student? Student { get; set; }
}

