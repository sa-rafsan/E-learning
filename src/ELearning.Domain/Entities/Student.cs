namespace ELearning.Domain.Entities;

public class Student
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string StudentId { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public string? Batch { get; set; }

    public string? Section { get; set; }

    public string? Department { get; set; }

    public string? ImagePath { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAtUtc { get; set; }
}

