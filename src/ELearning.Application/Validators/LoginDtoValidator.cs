using ELearning.Application.DTOs;
using ELearning.Domain.Enums;
using FluentValidation;

namespace ELearning.Application.Validators;

public sealed class LoginDtoValidator : AbstractValidator<LoginDto>
{
    public LoginDtoValidator()
    {
        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Role is required.")
            .Must(BeValidRole).WithMessage("Role must be Admin, Teacher, or Student.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Please enter a valid email address.")
            .MaximumLength(256);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");
    }

    private static bool BeValidRole(string? role)
    {
        return !string.IsNullOrWhiteSpace(role) &&
               Enum.TryParse<UserRole>(role.Trim(), ignoreCase: true, out var parsed) &&
               Enum.IsDefined(parsed);
    }
}

