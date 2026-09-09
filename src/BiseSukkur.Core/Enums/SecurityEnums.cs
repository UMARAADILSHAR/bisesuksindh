namespace BiseSukkur.Core.Enums;

public enum PermissionScopeType
{
    Board,
    District,
    School
}

public enum ApprovalStatus
{
    Pending,
    Approved,
    Rejected,
    Cancelled
}

public static class Permissions
{
    public const string UsersView = "Users.View";
    public const string UsersManage = "Users.Manage";
    public const string EnrollmentView = "Enrollment.View";
    public const string EnrollmentManage = "Enrollment.Manage";
    public const string EnrollmentApprove = "Enrollment.Approve";
    public const string ExaminationManage = "Examination.Manage";
    public const string ResultsEnter = "Results.Enter";
    public const string ResultsApprove = "Results.Approve";
    public const string FinanceView = "Finance.View";
    public const string FinanceVerify = "Finance.Verify";
    public const string CertificatesIssue = "Certificates.Issue";
    public const string ReportsView = "Reports.View";
    public const string ReportsExport = "Reports.Export";
    public const string AuditView = "Audit.View";
    public const string SystemManage = "System.Manage";
}
