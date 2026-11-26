using ELearning.Application.DTOs;
using FluentValidation;

namespace ELearning.Application.Validators;

public sealed class CreateCourseDtoValidator : AbstractValidator<CreateCourseDto>
{
    public CreateCourseDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Course Title is required.")
            .MaximumLength(160).WithMessage("Title cannot exceed 160 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters.");

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Category is required.")
            .MaximumLength(80).WithMessage("Category cannot exceed 80 characters.");

        RuleFor(x => x.DurationHours)
            .GreaterThan(0).WithMessage("Duration must be at least 1 hour.");
    }
}

