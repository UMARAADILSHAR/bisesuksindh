using BiseSukkur.Core.Common;
using BiseSukkur.Core.DTOs;
using BiseSukkur.Core.Interfaces;
using MediatR;

namespace BiseSukkur.Application.Commands.Challan;

public record VerifyChallanCommand(
    int InvoiceId,
    string BankTransactionRef,
    DateTime PaymentDate,
    string PaymentMethod
) : IRequest<Result<bool>>;

public class VerifyChallanCommandHandler : IRequestHandler<VerifyChallanCommand, Result<bool>>
{
    private readonly IChallanService _challanService;
    private readonly ICurrentUserService _currentUser;

    public VerifyChallanCommandHandler(IChallanService challanService, ICurrentUserService currentUser)
    {
        _challanService = challanService;
        _currentUser = currentUser;
    }

    public async Task<Result<bool>> Handle(VerifyChallanCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsSuperAdmin)
        {
            return Result.Failure<bool>(Error.Unauthorized);
        }

        var verifiedByUsername = _currentUser.Username;
        if (string.IsNullOrWhiteSpace(verifiedByUsername))
        {
            return Result.Failure<bool>("Challan.VerifierUnavailable", "The current verifying user could not be identified.");
        }

        try
        {
            var verification = await _challanService.VerifyAndMarkAsPaidAsync(
                request.InvoiceId,
                new PaymentVerificationRequestDto
                {
                    PaymentReference = request.BankTransactionRef,
                    PaymentReceivedAt = request.PaymentDate,
                    PaymentMethod = request.PaymentMethod
                },
                verifiedByUsername
            );

            if (!verification.Success)
            {
                return Result.Failure<bool>("Challan.NotFoundOrInvalid", verification.ErrorMessage ?? "Unable to verify the challan payment.");
            }

            return Result.Success(true);
        }
        catch (Exception)
        {
            return Result.Failure<bool>("Challan.VerificationFailed", "Unable to verify the challan payment.");
        }
    }
}
