using System.Security.Claims;

namespace BiseSukkur.Core.Interfaces;

public interface IPdfAccessService
{
    Task<bool> CanAccessEnrollmentCardAsync(ClaimsPrincipal user, int enrollmentId);
    Task<bool> CanAccessRollSlipAsync(ClaimsPrincipal user, int examinationFormId);
    Task<bool> CanAccessCertificateAsync(ClaimsPrincipal user, int resultId);
    Task<bool> CanAccessInvoicePdfAsync(ClaimsPrincipal user, int invoiceId);
}
