using BiseSukkur.Core.DTOs;
using BiseSukkur.Core.Entities;
using BiseSukkur.Core.Enums;

namespace BiseSukkur.Core.Interfaces;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<bool> ChangePasswordAsync(ChangePasswordRequest request);
    Task<User?> GetUserByIdAsync(int id);
    string HashPassword(string password);
    bool VerifyPassword(string password, string passwordHash);
}

public interface IBoardOperationsService
{
    Task<List<ExaminationSession>> GetExaminationSessionsAsync(bool includeArchived = false);
    Task<ExaminationSession> SaveExaminationSessionAsync(ExaminationSession session);
    Task<List<SubjectScheme>> GetSubjectSchemesAsync(int sessionId, string? classLevel = null, string? group = null);
    Task<SubjectScheme> SaveSubjectSchemeAsync(SubjectScheme scheme);
    Task<SchoolAffiliationApplication> SubmitAffiliationAsync(SchoolAffiliationApplication application, int userId, string username);
    Task<bool> ReviewAffiliationAsync(long applicationId, AffiliationStatus status, int reviewerId, string reviewerUsername, string? comment = null);
    Task<SchoolInspection> AddInspectionAsync(SchoolInspection inspection, int userId, string username);
    Task<ResultRecheckRequest> SubmitRecheckAsync(ResultRecheckRequest request);
    Task<bool> ReviewRecheckAsync(long requestId, RecheckStatus status, int reviewerId, string reviewerUsername, string? comment = null);
    Task<ResultCorrectionRequest> SubmitCorrectionAsync(ResultCorrectionRequest request);
    Task<bool> ReviewCorrectionAsync(long requestId, CorrectionStatus status, int reviewerId, string reviewerUsername, string? comment = null);
    Task<bool> ChangeCertificateStatusAsync(int certificateId, CertificateStatus status, string reason, int userId, string username);
    Task<List<CertificateStatusHistory>> GetCertificateHistoryAsync(int certificateId);
}

public interface IPermissionService
{
    Task<bool> HasPermissionAsync(string permission, int? schoolId = null, int? districtId = null);
    Task<bool> HasPermissionAsync(int userId, string permission, int? schoolId = null, int? districtId = null);
}

public interface IMfaService
{
    string GenerateSecret();
    string BuildOtpAuthUri(string username, string secret);
    bool VerifyCode(string secret, string code, DateTimeOffset? now = null);
}

public interface IApprovalService
{
    Task<ApprovalRequest> SubmitAsync(string requestType, string entityType, string entityId, string payloadJson, int userId, string username);
    Task<bool> ReviewAsync(long approvalId, ApprovalStatus status, int reviewerId, string reviewerUsername, string? comment = null);
}

public interface ISecurityAuditService
{
    Task RecordAsync(string eventType, string action, int? userId = null, string? username = null, string? entityType = null, string? entityId = null, object? details = null);
}

public interface IEnrollmentService
{
    Task<List<Enrollment>> GetEnrollmentsBySchoolAsync(int schoolId, int? academicYearId = null, string? search = null, string? classLevel = null);
    Task<PagedResult<Enrollment>> GetPagedEnrollmentsAsync(int schoolId, PagedRequest request);
    Task<EnrollmentMetricsDto> GetEnrollmentMetricsAsync(int schoolId, int? academicYearId = null);
    Task<Enrollment?> GetEnrollmentByIdAsync(int id);
    Task<Enrollment?> GetEnrollmentByCnicAsync(string cnic, int academicYearId);
    Task<Enrollment> SaveEnrollmentAsync(Enrollment enrollment, bool finalize = false);
    Task<string> SaveEnrollmentPhotoAsync(int schoolId, int enrollmentId, byte[] fileBytes, string contentType, string originalFileName);
    Task<bool> DeleteEnrollmentAsync(int id, int schoolId);
    Task<bool> CheckCnicExistsAsync(string cnic, int academicYearId, int? excludeEnrollmentId = null);
    Task<Enrollment?> LookupPreviousSscRecordAsync(string rollNumber, string? year = null);
}

public interface IEnrollmentNumberService
{
    string ResolveGroupCode(string group);
    string ResolveDistrictCode(District? district);
    Task<int> AllotNumbersForInvoiceAsync(int invoiceId, string username);
    Task<string> AllotSingleEnrollmentNumberAsync(int enrollmentId, string username);
}

public interface IChallanService
{
    Task<List<string>> GetEligibleClassesAsync(int schoolId, int academicYearId, string challanType);
    Task<List<string>> GetEligibleGroupsAsync(int schoolId, int academicYearId, string challanType, string classLevel);
    Task<List<string>> GetEligibleStudentTypesAsync(int schoolId, int academicYearId, string challanType, string classLevel, string group);
    Task<FeeRateResultDto> GetFeeRateAsync(int schoolId, string challanType, string classLevel, string group, string studentType, int academicYearId);
    Task<List<StudentSelectionDto>> GetEligibleStudentsAsync(int schoolId, int academicYearId, string challanType, string classLevel, string group, string studentType);
    Task<ChallanGenerationResultDto> GenerateChallanAsync(ChallanGenerationRequestDto request);
    Task<Invoice?> GetInvoiceByIdAsync(int id);
    Task<Invoice?> GetInvoiceByUuidAsync(Guid uuid);
    Task<List<Invoice>> GetInvoicesBySchoolAsync(int schoolId, int? academicYearId = null);
    Task<int> GetPendingInvoiceCountAsync(int schoolId, int? academicYearId = null);
    Task<PagedResult<Invoice>> GetPagedInvoicesAsync(int schoolId, PagedRequest request);
    Task<InvoiceMetricsDto> GetInvoiceMetricsAsync(int schoolId, int? academicYearId = null);
    Task<List<Invoice>> GetAllInvoicesAsync(string? search = null, InvoiceStatus? status = null, int? academicYearId = null);
    Task<PaymentVerificationResultDto> VerifyAndMarkAsPaidAsync(
        int invoiceId,
        PaymentVerificationRequestDto payment,
        string verifiedByUsername);
    Task<bool> RejectChallanAsync(int invoiceId, string reason, string rejectedByUsername);
    Task<bool> CancelChallanAsync(int invoiceId, string cancelledByUsername);
}

public interface IExaminationService
{
    Task<List<ExaminationForm>> GetExaminationFormsBySchoolAsync(int schoolId, int? academicYearId = null, string? search = null);
    Task<PagedResult<ExaminationForm>> GetPagedExaminationFormsAsync(int schoolId, PagedRequest request);
    Task<ExaminationMetricsDto> GetExaminationMetricsAsync(int schoolId, int? academicYearId = null);
    Task<ExaminationForm?> GetExaminationFormByIdAsync(int id);
    Task<ExaminationForm?> GetExaminationFormByEnrollmentIdAsync(int enrollmentId);
    Task<ExaminationForm> SaveExaminationFormAsync(ExaminationForm form, bool finalize = false);
    Task<List<GapReportRowDto>> GetGapReportAsync(int schoolId, int academicYearId);
    Task<List<ExamCenter>> GetExamCentersAsync(int? districtId = null);
    Task<ExamCenter> SaveExamCenterAsync(ExamCenter center);
    Task<bool> AssignSchoolToCenterAsync(int examCenterId, int schoolId);
    Task<List<ExamSchedule>> GetExamSchedulesAsync(int academicYearId, string? classLevel = null);
    Task<ExamSchedule> SaveExamScheduleAsync(ExamSchedule schedule);
    Task<bool> DeleteExamScheduleAsync(int scheduleId);
    Task<int> RunRollNumberAllotmentAsync(int academicYearId, string classLevel);
    Task<Result> SaveResultMarksAsync(int examinationFormId, Dictionary<string, int> marks, string declaredBy);
    List<string> GetSubjectCatalog(string classLevel, string group);
}

public interface IFeeRateService
{
    Task<List<FeeRate>> GetFeeRatesAsync(int academicYearId, string? feeType = null);
    Task<FeeRate> SaveFeeRateAsync(FeeRate rate);
    Task<bool> DeleteFeeRateAsync(int id);
    Task<bool> BulkUpdateFeesAsync(int academicYearId, string feeType, decimal multiplierOrAddition, bool isPercentage);
    Task<int> CopyRatesFromPreviousYearAsync(int sourceYearId, int targetYearId);
}

public interface IWindowService
{
    Task<AcademicYear?> GetActiveAcademicYearAsync();
    Task<List<AcademicYear>> GetAllAcademicYearsAsync();
    Task<AcademicYear> SaveAcademicYearAsync(AcademicYear year);
    Task<TimelineDto> ResolveTimelineAsync(int schoolId, string type = "enrollment");
    Task<TimelineDto> ResolveGlobalTimelineAsync(string type = "enrollment");
    Task<WindowStatusSummaryDto> GetWindowStatusSummaryAsync(string type = "enrollment", int? schoolId = null);
    Task<List<WindowOverride>> GetOverridesAsync(string? windowType = null);
    Task<WindowOverride> SaveOverrideAsync(WindowOverride windowOverride);
    Task<bool> DeleteOverrideAsync(int id);
}

public interface ICertificateService
{
    Task<Certificate?> GetCertificateByNumberAsync(string certNumber);
    Task<Certificate?> GetCertificateByTokenAsync(string token);
    Task<Certificate> GenerateCertificateAsync(int resultId);
    Task<List<Certificate>> GetCertificatesAsync(int? academicYearId = null, string? search = null);
    byte[] GenerateQrCodePng(string payload);
}

public interface IReportService
{
    Task<SuperAdminStatsDto> GetSuperAdminStatsAsync(int? academicYearId = null);
    Task<SchoolDashboardStatsDto> GetSchoolDashboardStatsAsync(int schoolId, int? academicYearId = null);
    Task<List<DistrictBreakdownDto>> GetDistrictBreakdownAsync(int? academicYearId = null);
}

public interface ISchoolService
{
    Task<List<School>> GetAllSchoolsAsync(int? districtId = null, string? search = null);
    Task<School?> GetSchoolByIdAsync(int id);
    Task<School> SaveSchoolAsync(School school);
    Task<bool> ToggleSchoolStatusAsync(int id);
    Task<List<District>> GetAllDistrictsAsync();
    Task<District?> GetDistrictByIdAsync(int id);
    Task<District> SaveDistrictAsync(District district);
    Task<List<Tehsil>> GetTehsilsByDistrictAsync(int districtId);
}

public interface IUserService
{
    Task<List<User>> GetAllUsersAsync(string? search = null, UserRole? role = null);
    Task<User?> GetUserByIdAsync(int id);
    Task<User> SaveUserAsync(User user, string? plainPasswordToSet = null);
    Task<bool> ToggleUserStatusAsync(int id);
    Task<bool> ResetPasswordAsync(int id, string newPassword);
}

public interface IActivityLogService
{
    Task LogAsync(string logName, string description, string? subjectType = null, int? subjectId = null, int? userId = null, string? username = null, object? properties = null);
    Task<List<ActivityLog>> GetRecentLogsAsync(int limit = 50);
}

public interface IPdfReportService
{
    Task<byte[]> GenerateEnrollmentCardPdfAsync(int enrollmentId);
    Task<byte[]> GenerateRollNumberSlipPdfAsync(int examFormId);
    Task<byte[]> GenerateCertificatePdfAsync(int certificateIdOrResultId);
    Task<byte[]> GenerateBankChallanPdfAsync(int invoiceId);
    Task<byte[]> GenerateCandidateListPdfAsync(int invoiceId);
}
