using ELearning.Domain.Enums;

namespace ELearning.Application.DTOs;

public sealed class SaveBatchDto
{
    public int CourseId { get; set; }

    public string BatchName { get; set; } = string.Empty;

    public string BatchCode { get; set; } = string.Empty;

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public string? Schedule { get; set; }

    public BatchStatus Status { get; set; } = BatchStatus.Active;

    public List<int> TeacherIds { get; set; } = [];

    public int? MasterBatchId { get; set; }

    public List<int>? MasterSectionIds { get; set; }
}

