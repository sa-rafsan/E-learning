namespace ELearning.Application.DTOs;

public sealed class CreateForumPostDto
{
    public int? ParentPostId { get; set; }

    public string Content { get; set; } = string.Empty;
}

