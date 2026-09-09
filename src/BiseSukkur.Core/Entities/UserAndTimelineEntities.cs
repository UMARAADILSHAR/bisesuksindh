using BiseSukkur.Core.Enums;

namespace BiseSukkur.Core.Entities;

public class User
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public UserRole Role { get; set; } = UserRole.SchoolAdmin;
    public int? SchoolId { get; set; }
    public School? School { get; set; }
    public int? DistrictId { get; set; }
    public District? District { get; set; }
    public bool IsActive { get; set; } = true;
    public bool MustChangePassword { get; set; } = false;
    public int FailedLoginAttempts { get; set; } = 0;
    public DateTime? LockoutEnd { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public bool MfaEnabled { get; set; }
    public string? MfaSecret { get; set; }
    public DateTime? MfaEnrolledAt { get; set; }
    public DateTime? MfaLastVerifiedAt { get; set; }
    public string? MfaRecoveryCodesHash { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class AcademicYear
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    public string YearName { get; set; } = "2026";
    public DateTime StartDate { get; set; } = new DateTime(2026, 1, 1);
    public DateTime EndDate { get; set; } = new DateTime(2026, 12, 31);
    public bool IsActive { get; set; } = true;

    // Enrollment Window
    public bool IsEnrollmentOpen { get; set; } = true;
    public DateTime? EnrollmentOpenStart { get; set; } = new DateTime(2026, 1, 1);
    public DateTime? EnrollmentOpenEnd { get; set; } = new DateTime(2026, 8, 31, 23, 59, 59);
    public DateTime? EnrollmentGraceEnd { get; set; } = new DateTime(2026, 9, 30, 23, 59, 59);
    public bool IsEnrollmentGraceEnabled { get; set; } = true;
    public string EnrollmentLateFeeType { get; set; } = "flat";
    public decimal EnrollmentLateFeeAmount { get; set; } = 800m;

    // Exam Window
    public bool IsExamOpen { get; set; } = true;
    public DateTime? ExamOpenStart { get; set; } = new DateTime(2026, 9, 1);
    public DateTime? ExamOpenEnd { get; set; } = new DateTime(2026, 11, 30, 23, 59, 59);
    public DateTime? ExamGraceEnd { get; set; } = new DateTime(2026, 12, 15, 23, 59, 59);
    public bool IsExamGraceEnabled { get; set; } = true;
    public string ExamLateFeeType { get; set; } = "flat";
    public decimal ExamLateFeeAmount { get; set; } = 1000m;

    public bool ExamTimetableAnnounced { get; set; } = false;
    public bool ResultsDeclared { get; set; } = false;
    public bool PromotionDone { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<FeeRate> FeeRates { get; set; } = new();
    public List<Enrollment> Enrollments { get; set; } = new();
    public List<Invoice> Invoices { get; set; } = new();
}

public class WindowOverride
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    public string ScopeType { get; set; } = "school"; // "school" or "district"
    public int ScopeId { get; set; }
    public string WindowType { get; set; } = "enrollment"; // "enrollment" or "exam"
    public DateTime? NormalStart { get; set; }
    public DateTime? NormalEnd { get; set; }
    public DateTime? GraceEnd { get; set; }
    public bool IsGraceEnabled { get; set; } = true;
    public string LateFeeType { get; set; } = "flat";
    public decimal LateFeeAmount { get; set; } = 0m;
    public string? Reason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
