namespace ELearning.Application.DTOs;

public sealed class StudentAttendanceSummaryDto
{
    public int SessionId { get; set; }

    public string CourseTitle { get; set; } = string.Empty;

    public string TeacherName { get; set; } = string.Empty;

    public DateOnly SessionDate { get; set; }

    public string Batch { get; set; } = string.Empty;

    public string Section { get; set; } = string.Empty;

    public string? Topic { get; set; }

    public string Status { get; set; } = string.Empty;

    public string? Remarks { get; set; }
}

