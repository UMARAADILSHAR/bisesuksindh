using BiseHyderabad.Core.Enums;

namespace BiseHyderabad.Core.Entities;

public class FeeRate
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    public int AcademicYearId { get; set; }
    public AcademicYear? AcademicYear { get; set; }
    public string FeeType { get; set; } = "enrollment"; // "enrollment" or "exam"
    public string ClassLevel { get; set; } = "SSC-I"; // "SSC-I", "SSC-II", "HSC-I", "HSC-II"
    public string GroupName { get; set; } = "Science"; // "Science", "General Regular", "Arts", "Pre-Medical", "Pre-Engineering", "Commerce", "Humanities"
    public string StudentType { get; set; } = "Regular"; // "Regular", "Private", "Repeater", "Reappear"
    public string FeeSlab { get; set; } = "public"; // "public" or "private"
    public decimal StandardFee { get; set; }
    public decimal LateFee { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class Enrollment
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    public Guid Uuid { get; set; } = Guid.NewGuid();
    public int SchoolId { get; set; }
    public School? School { get; set; }
    public int AcademicYearId { get; set; }
    public AcademicYear? AcademicYear { get; set; }

    public string StudentName { get; set; } = string.Empty;
    public string? FatherName { get; set; }
    public string? FatherCnic { get; set; }
    public string? Surname { get; set; }
    public string? GrNumber { get; set; } // General Register No (School Admission No)
    public string? IdentificationMark { get; set; }
    public string Cnic { get; set; } = string.Empty; // B-Form or CNIC
    public DateTime DateOfBirth { get; set; }
    public string? DateOfBirthWords { get; set; }
    public string Gender { get; set; } = "Male"; // Male / Female / Other
    public string Medium { get; set; } = "Urdu"; // English / Sindhi / Urdu
    public string Religion { get; set; } = "Islam";
    public string Nationality { get; set; } = "Pakistani";
    public string? Address { get; set; }
    public string? MobileNumber { get; set; }
    public string? PhotoPath { get; set; }

    // Academic
    public string ClassLevel { get; set; } = "SSC-I";
    public string Group { get; set; } = "Science";
    public string StudentType { get; set; } = "Regular";
    public string SubjectsJson { get; set; } = "[]";
    public DateTime? AdmissionDate { get; set; }
    public string? BoardRegNo { get; set; }       // Previous board registration number
    public string? EligibilityNo { get; set; }    // Eligibility certificate number
    public string? PreviousBoard { get; set; }    // e.g. "BISE Hyderabad" or custom
    public DateTime? BoardPassingDate { get; set; }

    public string? EnrollmentNumber { get; set; } // e.g. "E26SHY1-001-0001"
    public DateTime? EnrollmentNumberAllottedAt { get; set; }
    public int? InvoiceId { get; set; }
    public Invoice? Invoice { get; set; }
    public string? ChallanStatus { get; set; } // "generated", "paid", null
    public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Draft;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ExaminationForm? ExaminationForm { get; set; }
    public List<InvoiceItem> InvoiceItems { get; set; } = new();
}

public class Invoice
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    public Guid Uuid { get; set; } = Guid.NewGuid();
    public int SchoolId { get; set; }
    public School? School { get; set; }
    public int AcademicYearId { get; set; }
    public AcademicYear? AcademicYear { get; set; }

    public string InvoiceType { get; set; } = "enrollment"; // "enrollment" or "exam"
    public string InvoiceNumber { get; set; } = string.Empty; // e.g. "KH1-001-001"
    public string ClassGroupStudentType { get; set; } = string.Empty; // e.g. "SSC-I-Science-Regular"
    public int StudentCount { get; set; }
    public decimal Amount { get; set; }
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Pending;
    public string Phase { get; set; } = "normal"; // "normal" or "grace"
    public string? RejectionReason { get; set; }
    public string? PaymentReference { get; set; }
    public string? PaymentMethod { get; set; }
    public DateTime? PaymentReceivedAt { get; set; }
    public string? PaymentVerifiedByUsername { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public DateTime? VerifiedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<InvoiceItem> Items { get; set; } = new();
}

public class InvoiceItem
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    public int InvoiceId { get; set; }
    public Invoice? Invoice { get; set; }
    public int EnrollmentId { get; set; }
    public Enrollment? Enrollment { get; set; }
    public bool IsIncluded { get; set; } = true;
    public decimal FeeAmount { get; set; }
    public string InvoiceTypeSnapshot { get; set; } = "enrollment";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class InvoiceSequence
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    public string SequenceType { get; set; } = "enrollment"; // "invoice", "enrollment", "exam"
    public int? AcademicYearId { get; set; }
    public int? SchoolId { get; set; }
    public string? DistrictCode { get; set; }
    public int? Zone { get; set; }
    public string? GroupCode { get; set; }
    public string? Prefix { get; set; }
    public int LastNumber { get; set; } = 0;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
