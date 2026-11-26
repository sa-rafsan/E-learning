namespace ELearning.Application.DTOs;

public sealed record CourseDetailDto(
    int Id,
    string Title,
    string Description,
    string Instructor,
    string Category,
    int DurationHours,
    IReadOnlyList<LessonSummaryDto> Lessons);
