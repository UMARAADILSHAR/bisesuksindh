using BiseSukkur.Core.Entities;
using BiseSukkur.Core.Enums;

namespace BiseSukkur.Core.DTOs;

// --- Auth DTOs ---
public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? MfaCode { get; set; }
}

public class LoginResponse
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public UserSessionDto? User { get; set; }
    public bool MustChangePassword { get; set; }
    public bool MfaRequired { get; set; }
}

public class UserSessionDto
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public UserRole Role { get; set; }
    public int? SchoolId { get; set; }
    public string? SchoolName { get; set; }
    public int? DistrictId { get; set; }
    public string? DistrictName { get; set; }
    public bool MustChangePassword { get; set; }
    public bool MfaEnabled { get; set; }
}

public class ChangePasswordRequest
{
    public int UserId { get; set; }
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}

// --- Challan & Fee DTOs ---
public class FeeRateResultDto
{
    public decimal? FeeAmount { get; set; }
    public decimal? StandardFee { get; set; }
    public string Phase { get; set; } = "normal";
    public bool LateFeeApplies { get; set; }
    public string? ErrorMessage { get; set; }
}

public class StudentSelectionDto
{
    public int Id { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string? FatherName { get; set; }
    public string? Surname { get; set; }
    public string? GrNumber { get; set; }
    public string Cnic { get; set; } = string.Empty;
    public string? EnrollmentNumber { get; set; }
    public string ClassLevel { get; set; } = string.Empty;
    public string Group { get; set; } = string.Empty;
    public string StudentType { get; set; } = string.Empty;
    public bool IsIncluded { get; set; } = true;
    public bool PreviouslyExcluded { get; set; } = false;
}

public class ChallanGenerationRequestDto
{
    public int SchoolId { get; set; }
    public int AcademicYearId { get; set; }
    public string ChallanType { get; set; } = "enrollment"; // "enrollment" or "exam"
    public string ClassLevel { get; set; } = "SSC-I";
    public string Group { get; set; } = "Science";
    public string StudentType { get; set; } = "Regular";
    public List<StudentSelectionDto> Students { get; set; } = new();
    public string Username { get; set; } = string.Empty;
}

public class ChallanGenerationResultDto
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public int InvoiceId { get; set; }
    public Guid InvoiceUuid { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public int IncludedCount { get; set; }
    public int ExcludedCount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal FeePerStudent { get; set; }
    public string Phase { get; set; } = "normal";
}

// --- Timeline DTOs ---
public class TimelineDto
{
    public string Source { get; set; } = "global";
    public bool PortalOpen { get; set; } = true;
    public DateTime? NormalStart { get; set; }
    public DateTime? NormalEnd { get; set; }
    public DateTime? GraceEnd { get; set; }
    public bool GraceEnabled { get; set; } = true;
    public string LateFeeType { get; set; } = "flat";
    public decimal LateFeeAmount { get; set; } = 0m;
}

public class WindowStatusSummaryDto
{
    public string Phase { get; set; } = "normal"; // "normal", "grace", "closed"
    public string Label { get; set; } = "Open";
    public bool PortalOpen { get; set; } = true;
    public DateTime? EndsAt { get; set; }
    public string? Countdown { get; set; }
}

// --- Dashboard & Reports DTOs ---
public class SuperAdminStatsDto
{
    public int TotalSchools { get; set; }
    public int TotalEnrollments { get; set; }
    public int TotalExams { get; set; }
    public decimal TotalRevenue { get; set; }
    public int PendingInvoicesCount { get; set; }
    public decimal PendingInvoicesAmount { get; set; }
    public int VerifiedInvoicesCount { get; set; }
    public List<DistrictBreakdownDto> DistrictBreakdown { get; set; } = new();
    public List<ActivityLog> RecentActivities { get; set; } = new();
}

public class DistrictBreakdownDto
{
    public int DistrictId { get; set; }
    public string DistrictName { get; set; } = string.Empty;
    public string ShortCode { get; set; } = string.Empty;
    public int SchoolCount { get; set; }
    public int EnrollmentCount { get; set; }
    public int ExamCount { get; set; }
    public decimal TotalFeesCollected { get; set; }
}

public class SchoolDashboardStatsDto
{
    public int Ssc1Enrollments { get; set; }
    public int Ssc2Enrollments { get; set; }
    public int Hsc1Enrollments { get; set; }
    public int Hsc2Enrollments { get; set; }
    public int TotalEnrollments { get; set; }

    public int Ssc1Exams { get; set; }
    public int Ssc2Exams { get; set; }
    public int Hsc1Exams { get; set; }
    public int Hsc2Exams { get; set; }
    public int TotalExams { get; set; }

    public int TotalInvoices { get; set; }
    public decimal PendingInvoicesAmount { get; set; }
    public decimal PaidInvoicesAmount { get; set; }

    public WindowStatusSummaryDto EnrollmentWindow { get; set; } = new();
    public WindowStatusSummaryDto ExamWindow { get; set; } = new();
}

public class GapReportRowDto
{
    public int EnrollmentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string FatherName { get; set; } = string.Empty;
    public string EnrollmentNumber { get; set; } = string.Empty;
    public string ClassLevel { get; set; } = string.Empty;
    public string Group { get; set; } = string.Empty;
    public string StudentType { get; set; } = string.Empty;
    public bool HasExamForm { get; set; }
    public string? RollNumber { get; set; }
    public string ExamStatus { get; set; } = "Not Submitted";
}
