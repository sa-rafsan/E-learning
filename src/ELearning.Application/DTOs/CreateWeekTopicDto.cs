namespace ELearning.Application.DTOs;

public sealed class CreateWeekTopicDto
{
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int Order { get; set; } = 1;
}

