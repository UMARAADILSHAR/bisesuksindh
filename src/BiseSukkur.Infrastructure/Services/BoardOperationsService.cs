using BiseSukkur.Core.Entities;
using BiseSukkur.Core.Enums;
using BiseSukkur.Core.Interfaces;
using BiseSukkur.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace BiseSukkur.Infrastructure.Services;

public sealed class BoardOperationsService : IBoardOperationsService
{
    private readonly ApplicationDbContext _context;
    private readonly IPermissionService? _permissions;
    private readonly ISecurityAuditService? _audit;

    public BoardOperationsService(ApplicationDbContext context, IPermissionService? permissions = null, ISecurityAuditService? audit = null)
    {
        _context = context;
        _permissions = permissions;
        _audit = audit;
    }

    public async Task<List<ExaminationSession>> GetExaminationSessionsAsync(bool includeArchived = false)
    {
        var query = _context.ExaminationSessions.Include(s => s.AcademicYear).AsNoTracking();
        if (!includeArchived) query = query.Where(s => s.Status != ExaminationSessionStatus.Archived);
        return await query.OrderByDescending(s => s.RegistrationOpenAt).ToListAsync();
    }

    public async Task<ExaminationSession> SaveExaminationSessionAsync(ExaminationSession session)
    {
        await RequireSystemManagementAsync();
        ValidateSession(session);
        if (session.Id == 0)
        {
            if (string.IsNullOrWhiteSpace(session.Code)) session.Code = $"{session.AcademicYearId}-{Guid.NewGuid():N}"[..16].ToUpperInvariant();
            _context.ExaminationSessions.Add(session);
        }
        else
        {
            var existing = await _context.ExaminationSessions.FindAsync(session.Id) ?? throw new KeyNotFoundException("Examination session not found.");
            existing.Code = session.Code;
            existing.Name = session.Name;
            existing.AcademicYearId = session.AcademicYearId;
            existing.ExaminationType = session.ExaminationType;
            existing.ClassLevel = session.ClassLevel;
            existing.Status = session.Status;
            existing.RegistrationOpenAt = session.RegistrationOpenAt;
            existing.RegistrationCloseAt = session.RegistrationCloseAt;
            existing.ExaminationStartAt = session.ExaminationStartAt;
            existing.ExaminationEndAt = session.ExaminationEndAt;
            existing.ResultDeclarationAt = session.ResultDeclarationAt;
            existing.UpdatedAt = DateTime.UtcNow;
            session = existing;
        }
        await _context.SaveChangesAsync();
        await RecordAsync("session_saved", "ExaminationSession", session.Id.ToString(), new { session.Code, session.Status });
        return session;
    }

    public async Task<List<SubjectScheme>> GetSubjectSchemesAsync(int sessionId, string? classLevel = null, string? group = null)
    {
        var query = _context.SubjectSchemes.AsNoTracking().Where(s => s.ExaminationSessionId == sessionId && s.IsActive);
        if (!string.IsNullOrWhiteSpace(classLevel)) query = query.Where(s => s.ClassLevel == classLevel);
        if (!string.IsNullOrWhiteSpace(group)) query = query.Where(s => s.Group == group);
        return await query.OrderBy(s => s.SortOrder).ThenBy(s => s.SubjectCode).ToListAsync();
    }

    public async Task<SubjectScheme> SaveSubjectSchemeAsync(SubjectScheme scheme)
    {
        await RequireSystemManagementAsync();
        if (scheme.ExaminationSessionId <= 0 || string.IsNullOrWhiteSpace(scheme.SubjectCode) || string.IsNullOrWhiteSpace(scheme.SubjectName))
            throw new ArgumentException("Session, subject code, and subject name are required.");
        if (scheme.MaxMarks <= 0 || scheme.PassingMarks < 0 || scheme.PassingMarks > scheme.MaxMarks)
            throw new ArgumentException("Subject marks configuration is invalid.");
        if (scheme.HasPractical && (scheme.PracticalMaxMarks <= 0 || scheme.PracticalMaxMarks >= scheme.MaxMarks))
            throw new ArgumentException("Practical marks configuration is invalid.");

        if (scheme.Id == 0) _context.SubjectSchemes.Add(scheme);
        else
        {
            var existing = await _context.SubjectSchemes.FindAsync(scheme.Id) ?? throw new KeyNotFoundException("Subject scheme not found.");
            existing.ExaminationSessionId = scheme.ExaminationSessionId;
            existing.ClassLevel = scheme.ClassLevel;
            existing.Group = scheme.Group;
            existing.SubjectCode = scheme.SubjectCode;
            existing.SubjectName = scheme.SubjectName;
            existing.MaxMarks = scheme.MaxMarks;
            existing.PassingMarks = scheme.PassingMarks;
            existing.HasPractical = scheme.HasPractical;
            existing.PracticalMaxMarks = scheme.PracticalMaxMarks;
            existing.IsCompulsory = scheme.IsCompulsory;
            existing.IsActive = scheme.IsActive;
            existing.SortOrder = scheme.SortOrder;
            scheme = existing;
        }
        await _context.SaveChangesAsync();
        await RecordAsync("subject_scheme_saved", "SubjectScheme", scheme.Id.ToString(), new { scheme.SubjectCode, scheme.ExaminationSessionId });
        return scheme;
    }

    private static void ValidateSession(ExaminationSession session)
    {
        if (string.IsNullOrWhiteSpace(session.Name) || session.AcademicYearId <= 0 || string.IsNullOrWhiteSpace(session.ClassLevel))
            throw new ArgumentException("Session name, academic year, and class level are required.");
        if (session.RegistrationCloseAt <= session.RegistrationOpenAt)
            throw new ArgumentException("Registration close time must be after open time.");
        if (session.ExaminationStartAt.HasValue && session.ExaminationEndAt.HasValue && session.ExaminationEndAt <= session.ExaminationStartAt)
            throw new ArgumentException("Examination end time must be after start time.");
    }

    private async Task RequireSystemManagementAsync()
    {
        if (_permissions != null && !await _permissions.HasPermissionAsync(Permissions.SystemManage))
            throw new UnauthorizedAccessException("Only an authorized board administrator can change board configuration.");
    }

    private async Task RecordAsync(string action, string entityType, string entityId, object details)
    {
        if (_audit != null) await _audit.RecordAsync("board_operations", action, entityType: entityType, entityId: entityId, details: details);
    }

    public async Task<SchoolAffiliationApplication> SubmitAffiliationAsync(SchoolAffiliationApplication application, int userId, string username)
        => throw new NotImplementedException();

    public Task<bool> ReviewAffiliationAsync(long applicationId, AffiliationStatus status, int reviewerId, string reviewerUsername, string? comment = null)
        => throw new NotImplementedException();

    public Task<SchoolInspection> AddInspectionAsync(SchoolInspection inspection, int userId, string username)
        => throw new NotImplementedException();

    public Task<ResultRecheckRequest> SubmitRecheckAsync(ResultRecheckRequest request)
        => throw new NotImplementedException();

    public Task<bool> ReviewRecheckAsync(long requestId, RecheckStatus status, int reviewerId, string reviewerUsername, string? comment = null)
        => throw new NotImplementedException();

    public Task<ResultCorrectionRequest> SubmitCorrectionAsync(ResultCorrectionRequest request)
        => throw new NotImplementedException();

    public Task<bool> ReviewCorrectionAsync(long requestId, CorrectionStatus status, int reviewerId, string reviewerUsername, string? comment = null)
        => throw new NotImplementedException();

    public Task<bool> ChangeCertificateStatusAsync(int certificateId, CertificateStatus status, string reason, int userId, string username)
        => throw new NotImplementedException();

    public Task<List<CertificateStatusHistory>> GetCertificateHistoryAsync(int certificateId)
        => throw new NotImplementedException();
}
