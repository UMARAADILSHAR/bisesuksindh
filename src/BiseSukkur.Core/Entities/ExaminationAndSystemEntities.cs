using BiseSukkur.Core.Enums;

namespace BiseSukkur.Core.Entities;

public class ExamCenter
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    public string CenterCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int DistrictId { get; set; }
    public District? District { get; set; }
    public string? SuperintendentName { get; set; }
    public string? SuperintendentPhone { get; set; }
    public int Capacity { get; set; } = 500;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<ExamCenterSchool> AssignedSchools { get; set; } = new();
    public List<ExaminationForm> ExaminationForms { get; set; } = new();
}

public class ExamCenterSchool
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    public int ExamCenterId { get; set; }
    public ExamCenter? ExamCenter { get; set; }
    public int SchoolId { get; set; }
    public School? School { get; set; }
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
}

public class ExamSchedule
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    public int AcademicYearId { get; set; }
    public AcademicYear? AcademicYear { get; set; }
    public string ClassLevel { get; set; } = "SSC-I";
    public string Group { get; set; } = "Science";
    public string SubjectName { get; set; } = string.Empty;
    public DateTime ExamDate { get; set; }
    public string Session { get; set; } = "Morning"; // Morning (9:00 AM - 12:00 PM) / Evening (2:00 PM - 5:00 PM)
    public int PaperDurationMinutes { get; set; } = 180;
    public int MaxMarks { get; set; } = 100;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class ExaminationForm
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    public Guid Uuid { get; set; } = Guid.NewGuid();
    public int EnrollmentId { get; set; }
    public Enrollment? Enrollment { get; set; }
    public int SchoolId { get; set; }
    public School? School { get; set; }
    public int AcademicYearId { get; set; }
    public AcademicYear? AcademicYear { get; set; }

    public string ClassLevel { get; set; } = "SSC-I";
    public string Group { get; set; } = "Science";
    public string StudentType { get; set; } = "Regular";
    public string? PreviousSeatNumber { get; set; }
    public string? PreviousYear { get; set; }
    public string? RollNumber { get; set; } // Generated Seat / Roll Number e.g. "124501"
    public int? ExamCenterId { get; set; }
    public ExamCenter? ExamCenter { get; set; }

    public string SubjectsJson { get; set; } = "[]";
    public ExamFormStatus Status { get; set; } = ExamFormStatus.Draft;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Result? Result { get; set; }
}

public class Result
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    public int ExaminationFormId { get; set; }
    public ExaminationForm? ExaminationForm { get; set; }
    public string MarksJson { get; set; } = "{}"; // e.g. {"ENGLISH-I": 85, "PHYSICS-I": 90}
    public int TotalObtained { get; set; }
    public int TotalMaxMarks { get; set; }
    public decimal Percentage { get; set; }
    public string Grade { get; set; } = "A+"; // A+, A, B, C, D, E, Fail
    public bool IsPassed { get; set; } = true;
    public string? Remarks { get; set; }
    public DateTime DeclaredAt { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Certificate? Certificate { get; set; }
}

public class Certificate
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    public Guid Uuid { get; set; } = Guid.NewGuid();
    public int ResultId { get; set; }
    public Result? Result { get; set; }
    public string CertificateNumber { get; set; } = string.Empty; // e.g. "BISE-HYD-2026-00892"
    public string VerificationToken { get; set; } = string.Empty; // Unique token for public validation
    public string StudentName { get; set; } = string.Empty;
    public string FatherName { get; set; } = string.Empty;
    public string EnrollmentNumber { get; set; } = string.Empty;
    public string RollNumber { get; set; } = string.Empty;
    public string SchoolName { get; set; } = string.Empty;
    public string ClassLevel { get; set; } = "SSC-II";
    public string Group { get; set; } = "Science";
    public string Grade { get; set; } = "A+";
    public int TotalMarksObtained { get; set; }
    public int TotalMaxMarks { get; set; }
    public DateTime IssueDate { get; set; } = DateTime.UtcNow;
    public bool IsRevoked { get; set; } = false;
    public CertificateStatus Status { get; set; } = CertificateStatus.Issued;
    public string? RevocationReason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class ActivityLog
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    public string LogName { get; set; } = "default";
    public string Description { get; set; } = string.Empty;
    public string? SubjectType { get; set; }
    public int? SubjectId { get; set; }
    public int? UserId { get; set; }
    public string? CausedByUsername { get; set; }
    public string? PropertiesJson { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Setting
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
