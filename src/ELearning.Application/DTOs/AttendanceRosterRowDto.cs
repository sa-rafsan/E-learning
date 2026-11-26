namespace ELearning.Application.DTOs;

public sealed class AttendanceRosterRowDto
{
    public int StudentId { get; set; }

    public string StudentCode { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? Batch { get; set; }

    public string? Section { get; set; }

    public string Status { get; set; } = "Present";

    public string? Remarks { get; set; }
}

