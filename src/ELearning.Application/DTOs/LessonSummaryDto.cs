namespace ELearning.Application.DTOs;

public sealed record LessonSummaryDto(
    string Title,
    string Summary,
    int DurationMinutes,
    int Order);
