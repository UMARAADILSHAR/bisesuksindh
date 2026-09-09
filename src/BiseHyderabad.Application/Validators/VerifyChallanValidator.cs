using BiseHyderabad.Application.Commands.Challan;
using FluentValidation;

namespace BiseHyderabad.Application.Validators;

public sealed class VerifyChallanCommandValidator : AbstractValidator<VerifyChallanCommand>
{
    public VerifyChallanCommandValidator()
    {
        RuleFor(command => command.InvoiceId)
            .GreaterThan(0).WithMessage("A valid challan invoice is required.");

        RuleFor(command => command.BankTransactionRef)
            .NotEmpty().WithMessage("A bank transaction or receipt reference is required.")
            .MaximumLength(100).WithMessage("The payment reference cannot exceed 100 characters.");

        RuleFor(command => command.PaymentDate)
            .NotEqual(default(DateTime)).WithMessage("A payment date is required.")
            .LessThanOrEqualTo(_ => DateTime.UtcNow.Date).WithMessage("The payment date cannot be in the future.");

        RuleFor(command => command.PaymentMethod)
            .NotEmpty().WithMessage("A payment method is required.")
            .MaximumLength(50).WithMessage("The payment method cannot exceed 50 characters.");
    }
}
