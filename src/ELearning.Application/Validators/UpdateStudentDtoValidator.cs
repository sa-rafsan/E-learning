using ELearning.Application.DTOs;
using FluentValidation;

namespace ELearning.Application.Validators;

public sealed class UpdateStudentDtoValidator : AbstractValidator<UpdateStudentDto>
{
    public UpdateStudentDtoValidator()
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

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid account status.");
    }
}

