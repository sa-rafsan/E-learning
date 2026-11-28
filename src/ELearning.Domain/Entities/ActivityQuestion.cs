using ELearning.Domain.Enums;

namespace ELearning.Domain.Entities;

public class ActivityQuestion
{
    public int Id { get; set; }

    public int WeekActivityId { get; set; }

    public string QuestionText { get; set; } = string.Empty;

    public QuestionType Type { get; set; } = QuestionType.Mcq;

    public string? OptionA { get; set; }

    public string? OptionB { get; set; }

    public string? OptionC { get; set; }

    public string? OptionD { get; set; }

    public string? CorrectOption { get; set; }

    public decimal Marks { get; set; } = 1;

    public int Order { get; set; }

    public WeekActivity? Activity { get; set; }
}

