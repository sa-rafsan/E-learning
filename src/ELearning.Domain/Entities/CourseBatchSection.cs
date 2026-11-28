namespace ELearning.Domain.Entities;

public sealed class CourseBatchSection
{
    public int Id { get; set; }
    public int CourseBatchId { get; set; }
    public CourseBatch? CourseBatch { get; set; }
    public int MasterSectionId { get; set; }
    public MasterSection? MasterSection { get; set; }
}

