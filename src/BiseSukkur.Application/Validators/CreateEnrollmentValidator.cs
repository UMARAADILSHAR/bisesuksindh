using BiseSukkur.Application.Commands.Enrollment;
using FluentValidation;
using System.Text.RegularExpressions;

namespace BiseSukkur.Application.Validators;

public class CreateEnrollmentValidator : AbstractValidator<CreateEnrollmentCommand>
{
    public CreateEnrollmentValidator()
    {
        RuleFor(x => x.SchoolId)
            .GreaterThan(0).WithMessage("Valid School ID is required.");

        RuleFor(x => x.AcademicYearId)
            .GreaterThan(0).WithMessage("Valid Academic Year is required.");

        RuleFor(x => x.StudentName)
            .NotEmpty().WithMessage("Student Name is required.")
            .MaximumLength(150).WithMessage("Student Name cannot exceed 150 characters.");

        RuleFor(x => x.FatherName)
            .NotEmpty().WithMessage("Father Name is required.")
            .MaximumLength(150).WithMessage("Father Name cannot exceed 150 characters.");

        RuleFor(x => x.GrNumber)
            .NotEmpty().WithMessage("General Register (GR) Number is required.")
            .MaximumLength(50).WithMessage("GR Number cannot exceed 50 characters.");

        RuleFor(x => x.Cnic)
            .NotEmpty().WithMessage("CNIC / B-Form number is required.")
            .Matches(@"^\d{5}-\d{7}-\d{1}$").WithMessage("CNIC / B-Form must be in format 00000-0000000-0.");

        RuleFor(x => x.DateOfBirth)
            .NotEmpty().WithMessage("Date of Birth is required.")
            .Must(dob => dob <= DateTime.Today.AddYears(-10)).WithMessage("Candidate must be at least 10 years of age.");

        RuleFor(x => x.ClassLevel)
            .NotEmpty().WithMessage("Class level is required.")
            .Must(c => new[] { "SSC-I", "SSC-II", "HSC-I", "HSC-II" }.Contains(c))
            .WithMessage("Class level must be SSC-I, SSC-II, HSC-I, or HSC-II.");

        RuleFor(x => x.Group)
            .NotEmpty().WithMessage("Study Group is required.");

        RuleFor(x => x.Subjects)
            .NotNull().WithMessage("Subjects list cannot be null.")
            .Must(s => s != null && s.Count >= 5).WithMessage("A candidate must be enrolled in at least 5 subjects.");
    }
}
