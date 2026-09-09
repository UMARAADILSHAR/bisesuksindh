using BiseSukkur.Application.Commands.Challan;
using BiseSukkur.Core.DTOs;
using FluentValidation;

namespace BiseSukkur.Application.Validators;

public class GenerateChallanValidator : AbstractValidator<GenerateChallanCommand>
{
    public GenerateChallanValidator()
    {
        RuleFor(x => x.Request).NotNull().WithMessage("Challan request cannot be null.");

        When(x => x.Request != null, () =>
        {
            RuleFor(x => x.Request.SchoolId)
                .GreaterThan(0).WithMessage("Valid School ID is required.");

            RuleFor(x => x.Request.AcademicYearId)
                .GreaterThan(0).WithMessage("Valid Academic Year is required.");

            RuleFor(x => x.Request.ChallanType)
                .NotEmpty().WithMessage("Challan type is required.")
                .Must(t => new[] { "enrollment", "exam", "certificate" }.Contains(t.ToLower()))
                .WithMessage("Challan type must be 'enrollment', 'exam', or 'certificate'.");

            RuleFor(x => x.Request.Students)
                .NotEmpty().WithMessage("At least one candidate must be selected for challan generation.")
                .Must(list => list != null && list.Count(s => s.IsIncluded) <= 500).WithMessage("Cannot generate a single challan for more than 500 candidates.");

            RuleFor(x => x.Request.Username)
                .NotEmpty().WithMessage("Generator username is required.");
        });
    }
}

public class ChallanGenerationRequestDtoValidator : AbstractValidator<ChallanGenerationRequestDto>
{
    public ChallanGenerationRequestDtoValidator()
    {
        RuleFor(x => x.SchoolId).GreaterThan(0).WithMessage("Valid School ID is required.");
        RuleFor(x => x.AcademicYearId).GreaterThan(0).WithMessage("Valid Academic Year is required.");
        RuleFor(x => x.ChallanType).NotEmpty().WithMessage("Challan type is required.");
        RuleFor(x => x.Students).NotEmpty().WithMessage("At least one candidate is required.");
        RuleFor(x => x.Username).NotEmpty().WithMessage("Username is required.");
    }
}
