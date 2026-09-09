namespace BiseHyderabad.Core.Enums;

public enum UserRole
{
    SuperAdmin,
    DistrictAdmin,
    SchoolAdmin
}

public enum ExaminationSessionStatus
{
    Planned,
    Open,
    Closed,
    Archived
}

public enum AffiliationStatus
{
    Draft,
    Submitted,
    UnderReview,
    Approved,
    Rejected,
    Suspended,
    Expired
}

public enum RecheckStatus
{
    Submitted,
    UnderReview,
    Approved,
    Rejected,
    Completed
}

public enum CorrectionStatus
{
    Submitted,
    UnderReview,
    Approved,
    Rejected,
    Applied
}

public enum CertificateStatus
{
    Issued,
    Reissued,
    Revoked
}

public enum SchoolType
{
    Public,
    Private
}

public enum StudentType
{
    Regular,
    Private,
    Repeater,
    Reappear
}

public enum WindowPhase
{
    Normal,
    Grace,
    Closed
}

public enum InvoiceStatus
{
    Pending,
    Verified,
    Rejected,
    Cancelled
}

public enum EnrollmentStatus
{
    Draft,
    Final,
    Verified
}

public enum ExamFormStatus
{
    Draft,
    Final,
    Verified
}
