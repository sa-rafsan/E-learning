namespace ELearning.Domain.Entities;

public class TeacherQualification
{
    public int Id { get; set; }

    public int TeacherId { get; set; }

    public string DegreeName { get; set; } = string.Empty;

    public string InstitutionName { get; set; } = string.Empty;

    public int? PassingYear { get; set; }

    public Teacher? Teacher { get; set; }
}

