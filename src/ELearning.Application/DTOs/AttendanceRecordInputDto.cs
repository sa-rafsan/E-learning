using ELearning.Domain.Enums;

namespace ELearning.Application.DTOs;

public sealed class AttendanceRecordInputDto
{
    public int StudentId { get; set; }

    public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;

    public string? Remarks { get; set; }
}

