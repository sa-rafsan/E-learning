using ELearning.Application.DTOs;
using FluentValidation;

namespace ELearning.Application.Validators;

public sealed class SaveAttendanceRosterDtoValidator : AbstractValidator<SaveAttendanceRosterDto>
{
    public SaveAttendanceRosterDtoValidator()
    {
        RuleFor(x => x.CourseId)
            .GreaterThan(0).WithMessage("Course is required.");

        RuleFor(x => x.SessionDate)
            .NotEmpty().WithMessage("Session date is required.");

        RuleFor(x => x.Batch)
            .NotEmpty().WithMessage("Batch is required.")
            .MaximumLength(32);

        RuleFor(x => x.Section)
            .NotEmpty().WithMessage("Section is required.")
            .MaximumLength(16);

        RuleFor(x => x.Topic)
            .MaximumLength(200);

        RuleFor(x => x.Records)
            .NotEmpty().WithMessage("At least one student attendance record is required.");

        RuleForEach(x => x.Records).ChildRules(record =>
        {
            record.RuleFor(r => r.StudentId).GreaterThan(0);
            record.RuleFor(r => r.Status).IsInEnum().WithMessage("Invalid attendance status.");
            record.RuleFor(r => r.Remarks).MaximumLength(300);
        });
    }
}

