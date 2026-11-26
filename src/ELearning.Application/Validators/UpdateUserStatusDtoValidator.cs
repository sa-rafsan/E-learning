using ELearning.Application.DTOs;
using FluentValidation;

namespace ELearning.Application.Validators;

public sealed class UpdateUserStatusDtoValidator : AbstractValidator<UpdateUserStatusDto>
{
    public UpdateUserStatusDtoValidator()
    {
        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Status must be Active or Inactive.");
    }
}

