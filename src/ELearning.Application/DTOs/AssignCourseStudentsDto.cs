namespace ELearning.Application.DTOs;

public sealed class AssignCourseStudentsDto
{
    public List<int> StudentIds { get; set; } = [];

    public string? Batch { get; set; }

    public string? Section { get; set; }
}

