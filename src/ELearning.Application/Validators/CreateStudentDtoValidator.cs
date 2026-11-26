using ELearning.Application.DTOs;
using FluentValidation;

namespace ELearning.Application.Validators;

public sealed class CreateStudentDtoValidator : AbstractValidator<CreateStudentDto>
{
    public CreateStudentDtoValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full Name is required.")
            .MaximumLength(120).WithMessage("Full Name cannot exceed 120 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Please enter a valid email address.")
            .MaximumLength(256);

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(30).WithMessage("Phone Number cannot exceed 30 characters.");

        RuleFor(x => x.StudentId)
            .NotEmpty().WithMessage("Student ID is required.")
            .MaximumLength(64).WithMessage("Student ID cannot exceed 64 characters.");

        RuleFor(x => x.Department)
            .MaximumLength(120).WithMessage("Department cannot exceed 120 characters.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Confirm Password is required.")
            .Equal(x => x.Password).WithMessage("Confirm Password must match Password.");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid account status.");
    }
}

