using ELearning.Domain.Enums;

namespace ELearning.Application.DTOs;

public sealed class AdminCourseDto
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string CourseCode { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public int DurationHours { get; set; }

    public int DurationWeeks { get; set; }

    public string Status { get; set; } = CourseStatus.Active.ToString();

    public bool IsPublished { get; set; }

    public int? TeacherId { get; set; }

    public string TeacherName { get; set; } = string.Empty;

    public string? ImagePath { get; set; }

    public int EnrolledStudentsCount { get; set; }

    public int BatchesCount { get; set; }

    public int WeeksCount { get; set; }

    public IReadOnlyList<int> EnrolledStudentIds { get; set; } = [];
}
