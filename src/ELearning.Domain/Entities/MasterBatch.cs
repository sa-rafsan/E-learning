namespace ELearning.Domain.Entities;

public sealed class MasterBatch
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Sections { get; set; } = "A,B,C,D";
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}

