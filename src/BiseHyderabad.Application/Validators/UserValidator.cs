using BiseHyderabad.Core.Entities;
using FluentValidation;

namespace BiseHyderabad.Application.Validators;

public class UserValidator : AbstractValidator<User>
{
    public UserValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required.")
            .MinimumLength(3).WithMessage("Username must be at least 3 characters.")
            .MaximumLength(50).WithMessage("Username cannot exceed 50 characters.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Full Name is required.")
            .MaximumLength(100).WithMessage("Full Name cannot exceed 100 characters.");

        RuleFor(x => x.Email)
            .EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email))
            .WithMessage("A valid email address is required.");
    }
}

public class SchoolValidator : AbstractValidator<School>
{
    public SchoolValidator()
    {
        RuleFor(x => x.SemisCode)
            .NotEmpty().WithMessage("SEMIS code is required.")
            .MaximumLength(20).WithMessage("SEMIS code cannot exceed 20 characters.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("School board code is required.")
            .MaximumLength(10).WithMessage("School code cannot exceed 10 characters.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("School name is required.")
            .MaximumLength(200).WithMessage("School name cannot exceed 200 characters.");

        RuleFor(x => x.DistrictId)
            .GreaterThan(0).WithMessage("District assignment is required.");
    }
}
