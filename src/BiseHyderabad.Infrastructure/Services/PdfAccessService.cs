using System.Security.Claims;
using BiseHyderabad.Core.Interfaces;
using BiseHyderabad.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BiseHyderabad.Infrastructure.Services;

public class PdfAccessService : IPdfAccessService
{
    private readonly ApplicationDbContext _context;

    public PdfAccessService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> CanAccessEnrollmentCardAsync(ClaimsPrincipal user, int enrollmentId)
    {
        var enrollment = await _context.Enrollments
            .AsNoTracking()
            .Select(e => new { e.Id, e.SchoolId, e.TenantId })
            .FirstOrDefaultAsync(e => e.Id == enrollmentId);

        return enrollment != null && await CanAccessTenantAsync(user, enrollment.TenantId)
            && await CanAccessSchoolAsync(user, enrollment.SchoolId);
    }

    public async Task<bool> CanAccessRollSlipAsync(ClaimsPrincipal user, int examinationFormId)
    {
        var form = await _context.ExaminationForms
            .AsNoTracking()
            .Select(f => new { f.Id, f.SchoolId, f.TenantId })
            .FirstOrDefaultAsync(f => f.Id == examinationFormId);

        return form != null && await CanAccessTenantAsync(user, form.TenantId)
            && await CanAccessSchoolAsync(user, form.SchoolId);
    }

    public async Task<bool> CanAccessCertificateAsync(ClaimsPrincipal user, int resultId)
    {
        var schoolId = await _context.Results
            .AsNoTracking()
            .Where(r => r.Id == resultId)
            .Select(r => new { r.ExaminationForm!.SchoolId, r.TenantId })
            .FirstOrDefaultAsync();

        return schoolId != null && await CanAccessTenantAsync(user, schoolId.TenantId)
            && await CanAccessSchoolAsync(user, schoolId.SchoolId);
    }

    public async Task<bool> CanAccessInvoicePdfAsync(ClaimsPrincipal user, int invoiceId)
    {
        var invoice = await _context.Invoices
            .AsNoTracking()
            .Select(i => new { i.Id, i.SchoolId, i.TenantId })
            .FirstOrDefaultAsync(i => i.Id == invoiceId);

        return invoice != null && await CanAccessTenantAsync(user, invoice.TenantId)
            && await CanAccessSchoolAsync(user, invoice.SchoolId);
    }

    private static Task<bool> CanAccessTenantAsync(ClaimsPrincipal user, int tenantId)
    {
        if (user.IsInRole("SuperAdmin"))
        {
            return Task.FromResult(true);
        }

        return Task.FromResult(
            int.TryParse(user.FindFirst("TenantId")?.Value, out var userTenantId)
            && userTenantId == tenantId);
    }

    private async Task<bool> CanAccessSchoolAsync(ClaimsPrincipal user, int schoolId)
    {
        if (user.Identity?.IsAuthenticated != true)
        {
            return false;
        }

        if (user.IsInRole("SuperAdmin"))
        {
            return true;
        }

        if (user.IsInRole("SchoolAdmin"))
        {
            return int.TryParse(user.FindFirst("SchoolId")?.Value, out var userSchoolId)
                && userSchoolId == schoolId;
        }

        if (user.IsInRole("DistrictAdmin")
            && int.TryParse(user.FindFirst("DistrictId")?.Value, out var districtId))
        {
            return await _context.Schools
                .AsNoTracking()
                .AnyAsync(s => s.Id == schoolId && s.DistrictId == districtId);
        }

        return false;
    }
}
