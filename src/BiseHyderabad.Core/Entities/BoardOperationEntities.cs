using BiseHyderabad.Core.Enums;

namespace BiseHyderabad.Core.Entities;

public class ExaminationSession
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int AcademicYearId { get; set; }
    public AcademicYear? AcademicYear { get; set; }
    public string ExaminationType { get; set; } = "Annual";
    public string ClassLevel { get; set; } = "SSC-II";
    public ExaminationSessionStatus Status { get; set; } = ExaminationSessionStatus.Planned;
    public DateTime RegistrationOpenAt { get; set; }
    public DateTime RegistrationCloseAt { get; set; }
    public DateTime? ExaminationStartAt { get; set; }
    public DateTime? ExaminationEndAt { get; set; }
    public DateTime? ResultDeclarationAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class SubjectScheme
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    public int ExaminationSessionId { get; set; }
    public ExaminationSession? ExaminationSession { get; set; }
    public string ClassLevel { get; set; } = "SSC-II";
    public string Group { get; set; } = "Science";
    public string SubjectCode { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
    public int MaxMarks { get; set; } = 100;
    public int PassingMarks { get; set; } = 33;
    public bool HasPractical { get; set; }
    public int PracticalMaxMarks { get; set; }
    public bool IsCompulsory { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
}

public class SchoolAffiliationApplication
{
    public long Id { get; set; }
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    public int SchoolId { get; set; }
    public School? School { get; set; }
    public string ApplicationNumber { get; set; } = string.Empty;
    public string RequestedCategory { get; set; } = string.Empty;
    public string RequestedLevelsJson { get; set; } = "[]";
    public AffiliationStatus Status { get; set; } = AffiliationStatus.Draft;
    public DateTime? SubmittedAt { get; set; }
    public int? ReviewedByUserId { get; set; }
    public string? ReviewedByUsername { get; set; }
    public string? ReviewComment { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public DateTime? ValidUntil { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public List<SchoolInspection> Inspections { get; set; } = new();
}

public class SchoolInspection
{
    public long Id { get; set; }
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    public long AffiliationApplicationId { get; set; }
    public SchoolAffiliationApplication? AffiliationApplication { get; set; }
    public DateTime InspectedAt { get; set; } = DateTime.UtcNow;
    public int InspectorUserId { get; set; }
    public string InspectorUsername { get; set; } = string.Empty;
    public string Findings { get; set; } = string.Empty;
    public int? Capacity { get; set; }
    public bool Passed { get; set; }
    public string? AttachmentPath { get; set; }
}

public class ResultRecheckRequest
{
    public long Id { get; set; }
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    public int ResultId { get; set; }
    public Result? Result { get; set; }
    public string RequestNumber { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public RecheckStatus Status { get; set; } = RecheckStatus.Submitted;
    public int SubmittedByUserId { get; set; }
    public string SubmittedByUsername { get; set; } = string.Empty;
    public int? ReviewedByUserId { get; set; }
    public string? ReviewedByUsername { get; set; }
    public string? ReviewComment { get; set; }
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt { get; set; }
}

public class ResultCorrectionRequest
{
    public long Id { get; set; }
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    public int ResultId { get; set; }
    public Result? Result { get; set; }
    public string RequestNumber { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string ProposedMarksJson { get; set; } = "{}";
    public CorrectionStatus Status { get; set; } = CorrectionStatus.Submitted;
    public int SubmittedByUserId { get; set; }
    public string SubmittedByUsername { get; set; } = string.Empty;
    public int? ReviewedByUserId { get; set; }
    public string? ReviewedByUsername { get; set; }
    public string? ReviewComment { get; set; }
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt { get; set; }
    public DateTime? AppliedAt { get; set; }
}

public class CertificateStatusHistory
{
    public long Id { get; set; }
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    public int CertificateId { get; set; }
    public Certificate? Certificate { get; set; }
    public CertificateStatus FromStatus { get; set; }
    public CertificateStatus ToStatus { get; set; }
    public string Reason { get; set; } = string.Empty;
    public int ChangedByUserId { get; set; }
    public string ChangedByUsername { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
}
