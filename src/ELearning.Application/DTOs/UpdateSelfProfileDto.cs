namespace ELearning.Application.DTOs;

public sealed class UpdateSelfProfileDto
{
    public string FullName { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public string? Address { get; set; }

    public string? Nationality { get; set; }

    public string? Country { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string? Gender { get; set; }

    public string? EducationalBackground { get; set; }

    public string? Specialization { get; set; }

    public string? ProfessionalExperience { get; set; }

    public string? ShortBiography { get; set; }

    public List<TeacherQualificationDto>? Qualifications { get; set; }
}

