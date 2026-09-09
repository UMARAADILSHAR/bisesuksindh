using BiseSukkur.Core.Enums;
using BiseSukkur.Core.Entities;
using BiseSukkur.Core.Interfaces;
using BiseSukkur.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace BiseSukkur.Infrastructure.Services;

public sealed class PermissionService : IPermissionService
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public PermissionService(ApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<bool> HasPermissionAsync(string permission, int? schoolId = null, int? districtId = null)
        => _currentUser.UserId.HasValue && await HasPermissionAsync(_currentUser.UserId.Value, permission, schoolId, districtId);

    public async Task<bool> HasPermissionAsync(int userId, string permission, int? schoolId = null, int? districtId = null)
    {
        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);
        if (user == null) return false;

        var grants = await _context.PermissionGrants.AsNoTracking()
            .Where(g => g.UserId == userId && g.Permission == permission &&
                        (g.ExpiresAt == null || g.ExpiresAt > DateTime.UtcNow))
            .ToListAsync();

        var matchingGrant = grants
            .Where(g => g.ScopeType == PermissionScopeType.Board ||
                        (g.ScopeType == PermissionScopeType.District && districtId.HasValue && g.ScopeId == districtId) ||
                        (g.ScopeType == PermissionScopeType.School && schoolId.HasValue && g.ScopeId == schoolId))
            .OrderByDescending(g => g.ScopeType)
            .FirstOrDefault();

        if (matchingGrant != null) return matchingGrant.IsAllowed;
        return HasRoleDefault(user.Role, permission, schoolId, districtId, user.SchoolId, user.DistrictId);
    }

    private static bool HasRoleDefault(UserRole role, string permission, int? schoolId, int? districtId, int? userSchoolId, int? userDistrictId)
    {
        if (role == UserRole.SuperAdmin) return true;
        if (role == UserRole.DistrictAdmin && districtId.HasValue && districtId == userDistrictId)
            return permission is Permissions.ReportsView or Permissions.ReportsExport or Permissions.EnrollmentView or Permissions.ExaminationManage;
        if (role == UserRole.SchoolAdmin && schoolId.HasValue && schoolId == userSchoolId)
            return permission is Permissions.EnrollmentView or Permissions.EnrollmentManage or Permissions.ExaminationManage or Permissions.FinanceView or Permissions.ReportsView;
        return false;
    }
}

public sealed class MfaService : IMfaService
{
    public string GenerateSecret() => Base32Encode(RandomNumberGenerator.GetBytes(20));

    public string BuildOtpAuthUri(string username, string secret)
        => $"otpauth://totp/BISE%20Sukkur:{Uri.EscapeDataString(username)}?secret={secret}&issuer=BISE%20Sukkur&digits=6&period=30";

    public bool VerifyCode(string secret, string code, DateTimeOffset? now = null)
    {
        if (string.IsNullOrWhiteSpace(secret) || string.IsNullOrWhiteSpace(code) || code.Length != 6 || !code.All(char.IsDigit)) return false;
        var timestamp = (now ?? DateTimeOffset.UtcNow).ToUnixTimeSeconds() / 30;
        var provided = Convert.ToUInt32(code, 10);
        for (var offset = -1L; offset <= 1L; offset++)
        {
            var expected = GenerateCode(secret, timestamp + offset);
            if (CryptographicOperations.FixedTimeEquals(Encoding.ASCII.GetBytes(expected), Encoding.ASCII.GetBytes(code))) return true;
        }
        return false;
    }

    private static string GenerateCode(string secret, long counter)
    {
        var key = Base32Decode(secret);
        Span<byte> counterBytes = stackalloc byte[8];
        System.Buffers.Binary.BinaryPrimitives.WriteInt64BigEndian(counterBytes, counter);
        using var hmac = new HMACSHA1(key);
        var hash = hmac.ComputeHash(counterBytes.ToArray());
        var offset = hash[^1] & 0x0f;
        var binary = ((hash[offset] & 0x7f) << 24) | (hash[offset + 1] << 16) | (hash[offset + 2] << 8) | hash[offset + 3];
        return (binary % 1_000_000).ToString("D6");
    }

    private static string Base32Encode(byte[] bytes)
    {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        var result = new StringBuilder((bytes.Length + 4) / 5 * 8);
        var buffer = 0;
        var bits = 0;
        foreach (var value in bytes)
        {
            buffer = (buffer << 8) | value;
            bits += 8;
            while (bits >= 5)
            {
                bits -= 5;
                result.Append(alphabet[(buffer >> bits) & 31]);
            }
        }
        if (bits > 0) result.Append(alphabet[(buffer << (5 - bits)) & 31]);
        return result.ToString();
    }

    private static byte[] Base32Decode(string value)
    {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        var buffer = 0;
        var bits = 0;
        using var output = new MemoryStream();
        foreach (var character in value.TrimEnd('=').ToUpperInvariant())
        {
            var index = alphabet.IndexOf(character);
            if (index < 0) throw new FormatException("Invalid Base32 secret.");
            buffer = (buffer << 5) | index;
            bits += 5;
            if (bits >= 8)
            {
                bits -= 8;
                output.WriteByte((byte)(buffer >> bits));
            }
        }
        return output.ToArray();
    }
}

public sealed class ApprovalService : IApprovalService
{
    private readonly ApplicationDbContext _context;
    private readonly ISecurityAuditService _audit;

    public ApprovalService(ApplicationDbContext context, ISecurityAuditService audit)
    {
        _context = context;
        _audit = audit;
    }

    public async Task<ApprovalRequest> SubmitAsync(string requestType, string entityType, string entityId, string payloadJson, int userId, string username)
    {
        var request = new ApprovalRequest
        {
            RequestType = requestType,
            EntityType = entityType,
            EntityId = entityId,
            PayloadJson = payloadJson,
            SubmittedByUserId = userId,
            SubmittedByUsername = username
        };
        _context.ApprovalRequests.Add(request);
        await _context.SaveChangesAsync();
        await _audit.RecordAsync("approval", "submitted", userId, username, entityType, entityId);
        return request;
    }

    public async Task<bool> ReviewAsync(long approvalId, ApprovalStatus status, int reviewerId, string reviewerUsername, string? comment = null)
    {
        if (status is not (ApprovalStatus.Approved or ApprovalStatus.Rejected)) return false;
        var request = await _context.ApprovalRequests.FirstOrDefaultAsync(a => a.Id == approvalId && a.Status == ApprovalStatus.Pending);
        if (request == null || request.SubmittedByUserId == reviewerId) return false;
        request.Status = status;
        request.ReviewedByUserId = reviewerId;
        request.ReviewedByUsername = reviewerUsername;
        request.ReviewComment = comment;
        request.ReviewedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        await _audit.RecordAsync("approval", status.ToString().ToLowerInvariant(), reviewerId, reviewerUsername, request.EntityType, request.EntityId, new { requestId = approvalId, comment });
        return true;
    }
}

public sealed class SecurityAuditService : ISecurityAuditService
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITenantContext? _tenantContext;

    public SecurityAuditService(
        ApplicationDbContext context,
        IHttpContextAccessor httpContextAccessor,
        ITenantContext? tenantContext = null)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _tenantContext = tenantContext;
    }

    public async Task RecordAsync(string eventType, string action, int? userId = null, string? username = null, string? entityType = null, string? entityId = null, object? details = null)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var tenantId = _tenantContext?.TenantId;
        if (!tenantId.HasValue && userId.HasValue)
        {
            tenantId = await _context.Users
                .IgnoreQueryFilters()
                .Where(user => user.Id == userId.Value)
                .Select(user => (int?)user.TenantId)
                .SingleOrDefaultAsync();
        }

        if (!tenantId.HasValue)
        {
            return;
        }

        _context.SecurityAuditEvents.Add(new SecurityAuditEvent
        {
            TenantId = tenantId.Value,
            EventType = eventType,
            Action = action,
            UserId = userId,
            Username = username,
            EntityType = entityType,
            EntityId = entityId,
            IpAddress = httpContext?.Connection.RemoteIpAddress?.ToString(),
            CorrelationId = httpContext?.TraceIdentifier,
            DetailsJson = details == null ? null : JsonSerializer.Serialize(details)
        });
        await _context.SaveChangesAsync();
    }
}
