namespace ELearning.Application.DTOs;

public sealed class AdminCourseDto
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public int DurationHours { get; set; }

    public bool IsPublished { get; set; }

    public int? TeacherId { get; set; }

    public string TeacherName { get; set; } = string.Empty;

    public int EnrolledStudentsCount { get; set; }

    public IReadOnlyList<int> EnrolledStudentIds { get; set; } = [];
}

