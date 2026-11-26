namespace ELearning.Application.DTOs;

public sealed class SaveAttendanceRosterDto
{
    public int CourseId { get; set; }

    public DateOnly SessionDate { get; set; }

    public string Batch { get; set; } = string.Empty;

    public string Section { get; set; } = string.Empty;

    public string? Topic { get; set; }

    public List<AttendanceRecordInputDto> Records { get; set; } = [];
}

