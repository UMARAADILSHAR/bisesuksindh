using BiseSukkur.Core.DTOs;
using BiseSukkur.Core.Entities;
using BiseSukkur.Core.Enums;
using BiseSukkur.Core.Interfaces;
using BiseSukkur.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BiseSukkur.Infrastructure.Services;

public class SchoolService : ISchoolService
{
    private readonly ApplicationDbContext _context;
    private readonly IActivityLogService _activityLog;

    public SchoolService(ApplicationDbContext context, IActivityLogService activityLog)
    {
        _context = context;
        _activityLog = activityLog;
    }

    public async Task<List<School>> GetAllSchoolsAsync(int? districtId = null, string? search = null)
    {
        var query = _context.Schools
            .Include(s => s.District)
            .Include(s => s.Tehsil)
            .AsQueryable();

        if (districtId.HasValue)
        {
            query = query.Where(s => s.DistrictId == districtId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(s => s.Name.ToLower().Contains(term) || s.SemisCode.Contains(term) || (s.Code != null && s.Code.Contains(term)));
        }

        return await query.OrderBy(s => s.Name).ToListAsync();
    }

    public async Task<School?> GetSchoolByIdAsync(int id)
    {
        return await _context.Schools
            .Include(s => s.District)
            .Include(s => s.Tehsil)
            .Include(s => s.Users)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<School> SaveSchoolAsync(School school)
    {
        if (school.Id == 0)
        {
            _context.Schools.Add(school);
        }
        else
        {
            var existing = await _context.Schools.FindAsync(school.Id);
            if (existing != null)
            {
                existing.SemisCode = school.SemisCode;
                existing.Code = school.Code;
                existing.Name = school.Name;
                existing.DistrictId = school.DistrictId;
                existing.TehsilId = school.TehsilId;
                existing.Type = school.Type;
                existing.Zone = school.Zone;
                existing.Address = school.Address;
                existing.ContactNumber = school.ContactNumber;
                existing.HeadName = school.HeadName;
                existing.HeadPhone = school.HeadPhone;
                existing.HeadCnic = school.HeadCnic;
                existing.HeadEmail = school.HeadEmail;
                existing.AllowedLevelsJson = school.AllowedLevelsJson;
                existing.IsActive = school.IsActive;
                existing.UpdatedAt = DateTime.UtcNow;
                school = existing;
            }
        }
        await _context.SaveChangesAsync();
        await _activityLog.LogAsync("school_save", $"Saved school '{school.Name}' (SEMIS: {school.SemisCode}).", "School", school.Id);
        return school;
    }

    public async Task<bool> ToggleSchoolStatusAsync(int id)
    {
        var school = await _context.Schools.FindAsync(id);
        if (school == null) return false;

        school.IsActive = !school.IsActive;
        school.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        await _activityLog.LogAsync("school_toggle", $"Toggled school '{school.Name}' active status to {school.IsActive}.", "School", school.Id);
        return true;
    }

    public async Task<List<District>> GetAllDistrictsAsync()
    {
        return await _context.Districts
            .Include(d => d.Tehsils)
            .Include(d => d.Schools)
            .OrderBy(d => d.Name)
            .ToListAsync();
    }

    public async Task<District?> GetDistrictByIdAsync(int id)
    {
        return await _context.Districts
            .Include(d => d.Tehsils)
            .Include(d => d.Schools)
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<District> SaveDistrictAsync(District district)
    {
        if (district.Id == 0)
        {
            _context.Districts.Add(district);
        }
        else
        {
            var existing = await _context.Districts.FindAsync(district.Id);
            if (existing != null)
            {
                existing.Name = district.Name;
                existing.ShortCode = district.ShortCode;
                district = existing;
            }
        }
        await _context.SaveChangesAsync();
        return district;
    }

    public async Task<List<Tehsil>> GetTehsilsByDistrictAsync(int districtId)
    {
        return await _context.Tehsils
            .Where(t => t.DistrictId == districtId)
            .OrderBy(t => t.Name)
            .ToListAsync();
    }
}

public class ReportService : IReportService
{
    private readonly ApplicationDbContext _context;
    private readonly IWindowService _windowService;

    public ReportService(ApplicationDbContext context, IWindowService windowService)
    {
        _context = context;
        _windowService = windowService;
    }

    public async Task<SuperAdminStatsDto> GetSuperAdminStatsAsync(int? academicYearId = null)
    {
        var activeYear = academicYearId.HasValue
            ? await _context.AcademicYears.FindAsync(academicYearId.Value)
            : await _windowService.GetActiveAcademicYearAsync();

        int yearId = activeYear?.Id ?? 0;

        var totalSchools = await _context.Schools.CountAsync();
        var totalEnrollments = await _context.Enrollments.CountAsync(e => yearId == 0 || e.AcademicYearId == yearId);
        var totalExams = await _context.ExaminationForms.CountAsync(ef => yearId == 0 || ef.AcademicYearId == yearId);

        var verifiedInvoices = await _context.Invoices
            .Where(i => (yearId == 0 || i.AcademicYearId == yearId) && i.Status == InvoiceStatus.Verified)
            .ToListAsync();

        var pendingInvoices = await _context.Invoices
            .Where(i => (yearId == 0 || i.AcademicYearId == yearId) && i.Status == InvoiceStatus.Pending)
            .ToListAsync();

        var totalRevenue = verifiedInvoices.Sum(i => i.Amount);
        var pendingAmount = pendingInvoices.Sum(i => i.Amount);

        var districtBreakdown = await GetDistrictBreakdownAsync(yearId > 0 ? yearId : null);
        var recentActivities = await _context.ActivityLogs.OrderByDescending(l => l.CreatedAt).Take(10).ToListAsync();

        return new SuperAdminStatsDto
        {
            TotalSchools = totalSchools,
            TotalEnrollments = totalEnrollments,
            TotalExams = totalExams,
            TotalRevenue = totalRevenue,
            PendingInvoicesCount = pendingInvoices.Count,
            PendingInvoicesAmount = pendingAmount,
            VerifiedInvoicesCount = verifiedInvoices.Count,
            DistrictBreakdown = districtBreakdown,
            RecentActivities = recentActivities
        };
    }

    public async Task<SchoolDashboardStatsDto> GetSchoolDashboardStatsAsync(int schoolId, int? academicYearId = null)
    {
        var activeYear = academicYearId.HasValue
            ? await _context.AcademicYears.FindAsync(academicYearId.Value)
            : await _windowService.GetActiveAcademicYearAsync();

        int yearId = activeYear?.Id ?? 0;

        var enrollments = await _context.Enrollments
            .Where(e => e.SchoolId == schoolId && (yearId == 0 || e.AcademicYearId == yearId))
            .ToListAsync();

        var exams = await _context.ExaminationForms
            .Where(ef => ef.SchoolId == schoolId && (yearId == 0 || ef.AcademicYearId == yearId))
            .ToListAsync();

        var invoices = await _context.Invoices
            .Where(i => i.SchoolId == schoolId && (yearId == 0 || i.AcademicYearId == yearId))
            .ToListAsync();

        var enrollmentWindow = await _windowService.GetWindowStatusSummaryAsync("enrollment", schoolId);
        var examWindow = await _windowService.GetWindowStatusSummaryAsync("exam", schoolId);

        return new SchoolDashboardStatsDto
        {
            Ssc1Enrollments = enrollments.Count(e => e.ClassLevel.StartsWith("SSC-I") || e.ClassLevel == "9"),
            Ssc2Enrollments = enrollments.Count(e => e.ClassLevel.StartsWith("SSC-II") || e.ClassLevel == "10"),
            Hsc1Enrollments = enrollments.Count(e => e.ClassLevel.StartsWith("HSC-I") || e.ClassLevel == "11"),
            Hsc2Enrollments = enrollments.Count(e => e.ClassLevel.StartsWith("HSC-II") || e.ClassLevel == "12"),
            TotalEnrollments = enrollments.Count,

            Ssc1Exams = exams.Count(e => e.ClassLevel.StartsWith("SSC-I") || e.ClassLevel == "9"),
            Ssc2Exams = exams.Count(e => e.ClassLevel.StartsWith("SSC-II") || e.ClassLevel == "10"),
            Hsc1Exams = exams.Count(e => e.ClassLevel.StartsWith("HSC-I") || e.ClassLevel == "11"),
            Hsc2Exams = exams.Count(e => e.ClassLevel.StartsWith("HSC-II") || e.ClassLevel == "12"),
            TotalExams = exams.Count,

            TotalInvoices = invoices.Count,
            PendingInvoicesAmount = invoices.Where(i => i.Status == InvoiceStatus.Pending).Sum(i => i.Amount),
            PaidInvoicesAmount = invoices.Where(i => i.Status == InvoiceStatus.Verified).Sum(i => i.Amount),

            EnrollmentWindow = enrollmentWindow,
            ExamWindow = examWindow
        };
    }

    public async Task<List<DistrictBreakdownDto>> GetDistrictBreakdownAsync(int? academicYearId = null)
    {
        var districts = await _context.Districts
            .Include(d => d.Schools).ThenInclude(s => s.Enrollments)
            .Include(d => d.Schools).ThenInclude(s => s.Invoices)
            .OrderBy(d => d.Name)
            .ToListAsync();

        var list = new List<DistrictBreakdownDto>();
        foreach (var d in districts)
        {
            var schools = d.Schools;
            var enrollments = schools.SelectMany(s => s.Enrollments)
                .Where(e => !academicYearId.HasValue || e.AcademicYearId == academicYearId.Value)
                .ToList();

            var paidInvoices = schools.SelectMany(s => s.Invoices)
                .Where(i => (!academicYearId.HasValue || i.AcademicYearId == academicYearId.Value) && i.Status == InvoiceStatus.Verified)
                .ToList();

            var schoolIds = schools.Select(s => s.Id).ToList();
            var examsCount = await _context.ExaminationForms
                .CountAsync(ef => schoolIds.Contains(ef.SchoolId) && (!academicYearId.HasValue || ef.AcademicYearId == academicYearId.Value));

            list.Add(new DistrictBreakdownDto
            {
                DistrictId = d.Id,
                DistrictName = d.Name,
                ShortCode = d.ShortCode,
                SchoolCount = schools.Count,
                EnrollmentCount = enrollments.Count,
                ExamCount = examsCount,
                TotalFeesCollected = paidInvoices.Sum(i => i.Amount)
            });
        }
        return list;
    }
}
