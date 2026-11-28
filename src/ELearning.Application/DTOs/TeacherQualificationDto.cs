namespace ELearning.Application.DTOs;

public sealed class TeacherQualificationDto
{
    public string DegreeName { get; set; } = string.Empty;

    public string InstitutionName { get; set; } = string.Empty;

    public int? PassingYear { get; set; }
}

