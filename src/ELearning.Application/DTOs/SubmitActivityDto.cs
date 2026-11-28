namespace ELearning.Application.DTOs;

public sealed class SubmitActivityDto
{
    public string? SubmissionText { get; set; }

    public string? AttachmentPath { get; set; }

    public Dictionary<int, string>? Answers { get; set; }

    public DateTime? StartedAtUtc { get; set; }
}

