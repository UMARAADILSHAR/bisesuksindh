using BiseSukkur.Core.Entities;
using FluentValidation;

namespace BiseSukkur.Application.Validators;

public class ExaminationFormValidator : AbstractValidator<ExaminationForm>
{
    public ExaminationFormValidator()
    {
        RuleFor(x => x.SchoolId)
            .GreaterThan(0).WithMessage("Valid School ID is required.");

        RuleFor(x => x.EnrollmentId)
            .GreaterThan(0).WithMessage("Candidate enrollment ID is required.");

        RuleFor(x => x.AcademicYearId)
            .GreaterThan(0).WithMessage("Academic Year ID is required.");

        RuleFor(x => x.ClassLevel)
            .NotEmpty().WithMessage("Class Level is required.")
            .Must(c => new[] { "SSC-I", "SSC-II", "HSC-I", "HSC-II" }.Contains(c))
            .WithMessage("Class level must be SSC-I, SSC-II, HSC-I, or HSC-II.");

        RuleFor(x => x.Group)
            .NotEmpty().WithMessage("Group is required.");

        RuleFor(x => x.StudentType)
            .NotEmpty().WithMessage("Student Type is required.")
            .Must(t => new[] { "Regular", "Private", "Repeater", "Reappear" }.Contains(t))
            .WithMessage("Student Type must be Regular, Private, Repeater, or Reappear.");
    }
}
