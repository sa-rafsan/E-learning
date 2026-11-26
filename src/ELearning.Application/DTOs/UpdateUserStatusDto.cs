using ELearning.Domain.Enums;

namespace ELearning.Application.DTOs;

public sealed class UpdateUserStatusDto
{
    public UserStatus Status { get; set; }
}

