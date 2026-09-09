namespace BiseHyderabad.Core.DTOs;

/// <summary>
/// Evidence supplied by a board officer when reconciling a bank challan.
/// This is deliberately provider-neutral so a future payment gateway can use
/// the same reconciliation path.
/// </summary>
public sealed class PaymentVerificationRequestDto
{
    public string PaymentReference { get; set; } = string.Empty;
    public DateTime PaymentReceivedAt { get; set; } = DateTime.UtcNow.Date;
    public string PaymentMethod { get; set; } = "Bank deposit";
}

public sealed class PaymentVerificationResultDto
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public int InvoiceId { get; init; }
    public string? InvoiceNumber { get; init; }
    public int ReleasedCandidateCount { get; init; }

    public static PaymentVerificationResultDto Failed(string errorMessage) => new()
    {
        Success = false,
        ErrorMessage = errorMessage
    };
}
