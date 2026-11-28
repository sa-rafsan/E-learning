namespace ELearning.Domain.Entities;

public class Teacher
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public string? Department { get; set; }

    public string? Designation { get; set; }

    public string? ImagePath { get; set; }

    public string? Address { get; set; }

    public string? Nationality { get; set; }

    public string? Country { get; set; }

    public string? Specialization { get; set; }

    public string? ProfessionalExperience { get; set; }

    public string? ShortBiography { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAtUtc { get; set; }

    public ICollection<TeacherQualification> Qualifications { get; set; } = new List<TeacherQualification>();

    public ICollection<BatchTeacher> BatchAssignments { get; set; } = new List<BatchTeacher>();
}

