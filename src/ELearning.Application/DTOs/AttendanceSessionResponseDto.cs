namespace ELearning.Application.DTOs;

public sealed class AttendanceSessionResponseDto
{
    public int SessionId { get; set; }

    public int CourseId { get; set; }

    public string CourseTitle { get; set; } = string.Empty;

    public int TeacherId { get; set; }

    public string TeacherName { get; set; } = string.Empty;

    public DateOnly SessionDate { get; set; }

    public string Batch { get; set; } = string.Empty;

    public string Section { get; set; } = string.Empty;

    public string? Topic { get; set; }

    public int PresentCount { get; set; }

    public int AbsentCount { get; set; }

    public int LateCount { get; set; }

    public int ExcusedCount { get; set; }

    public IReadOnlyList<AttendanceRosterRowDto> Records { get; set; } = [];
}

