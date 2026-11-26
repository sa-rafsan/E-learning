namespace ELearning.Application.DTOs;

public sealed record CourseSummaryDto(
    int Id,
    string Title,
    string Description,
    string Instructor,
    string Category,
    int DurationHours,
    int LessonCount);
