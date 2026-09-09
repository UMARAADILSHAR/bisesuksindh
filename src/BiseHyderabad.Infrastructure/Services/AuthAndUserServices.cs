using BiseHyderabad.Core.DTOs;
using BiseHyderabad.Core.Entities;
using BiseHyderabad.Core.Enums;
using BiseHyderabad.Core.Interfaces;
using BiseHyderabad.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace BiseHyderabad.Infrastructure.Services;

public class AuthService : IAuthService
{
    private const int MaxFailedAttempts = 5;
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

    private readonly ApplicationDbContext _context;
    private readonly IActivityLogService _activityLog;
    private readonly IMemoryCache _cache;
    private readonly IIpRateLimitService _ipRateLimit;
    private readonly IMfaService? _mfaService;
    private readonly ISecurityAuditService? _securityAudit;

    public AuthService(
        ApplicationDbContext context,
        IActivityLogService activityLog,
        IMemoryCache cache,
        IIpRateLimitService ipRateLimit,
        IMfaService? mfaService = null,
        ISecurityAuditService? securityAudit = null)
    {
        _context = context;
        _activityLog = activityLog;
        _cache = cache;
        _ipRateLimit = ipRateLimit;
        _mfaService = mfaService;
        _securityAudit = securityAudit;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        if (_ipRateLimit.IsRateLimited("login", permitLimit: 10, window: TimeSpan.FromMinutes(5)))
        {
            return new LoginResponse
            {
                Success = false,
                ErrorMessage = "Too many login attempts from this network. Please wait a few minutes and try again."
            };
        }

        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return new LoginResponse { Success = false, ErrorMessage = "Username and password are required." };
        }

        var normalizedUsername = request.Username.Trim().ToLowerInvariant();
        var lockoutKey = $"login_lockout:{normalizedUsername}";

        if (_cache.TryGetValue(lockoutKey, out _))
        {
            return new LoginResponse
            {
                Success = false,
                ErrorMessage = "Account temporarily locked due to multiple failed login attempts. Please try again in 15 minutes."
            };
        }

        var user = await _context.Users
            .IgnoreQueryFilters()
            .Include(u => u.School)
            .Include(u => u.District)
            .FirstOrDefaultAsync(u => u.Username.ToLower() == normalizedUsername);

        if (user != null && user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTime.UtcNow)
        {
            var remaining = (user.LockoutEnd.Value - DateTime.UtcNow).TotalMinutes;
            return new LoginResponse
            {
                Success = false,
                ErrorMessage = $"Account is temporarily locked due to consecutive failed attempts. Please retry in {Math.Ceiling(remaining)} minute(s)."
            };
        }

        if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
        {
            if (user != null)
            {
                user.FailedLoginAttempts++;
                if (user.FailedLoginAttempts >= MaxFailedAttempts)
                {
                    user.LockoutEnd = DateTime.UtcNow.Add(LockoutDuration);
                }
                user.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
            RegisterFailedAttempt(normalizedUsername);
            return new LoginResponse { Success = false, ErrorMessage = "Invalid credentials. Please verify username and password." };
        }

        if (!user.IsActive)
        {
            return new LoginResponse { Success = false, ErrorMessage = "Your account has been deactivated. Please contact the Board Administrator." };
        }

        if (user.MfaEnabled)
        {
            if (_mfaService == null || string.IsNullOrWhiteSpace(request.MfaCode) ||
                string.IsNullOrWhiteSpace(user.MfaSecret) || !_mfaService.VerifyCode(user.MfaSecret, request.MfaCode))
            {
                return new LoginResponse
                {
                    Success = false,
                    MfaRequired = true,
                    ErrorMessage = "A valid authenticator verification code is required."
                };
            }
            user.MfaLastVerifiedAt = DateTime.UtcNow;
        }

        _cache.Remove($"login_failures:{normalizedUsername}");

        user.FailedLoginAttempts = 0;
        user.LockoutEnd = null;
        user.LastLoginAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        await _activityLog.LogAsync("auth_login", $"User '{user.Username}' logged in successfully.", "User", user.Id, user.Id, user.Username);
        if (_securityAudit != null)
            await _securityAudit.RecordAsync("authentication", "login_succeeded", user.Id, user.Username);

        return new LoginResponse
        {
            Success = true,
            MustChangePassword = user.MustChangePassword,
            User = new UserSessionDto
            {
                Id = user.Id,
                TenantId = user.TenantId,
                Username = user.Username,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role,
                SchoolId = user.SchoolId,
                SchoolName = user.School?.Name,
                DistrictId = user.DistrictId,
                DistrictName = user.District?.Name,
                MustChangePassword = user.MustChangePassword
                ,MfaEnabled = user.MfaEnabled
            }
        };
    }

    private void RegisterFailedAttempt(string normalizedUsername)
    {
        var failureKey = $"login_failures:{normalizedUsername}";
        var failures = _cache.GetOrCreate(failureKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = LockoutDuration;
            return 0;
        });

        failures++;
        _cache.Set(failureKey, failures, LockoutDuration);

        if (failures >= MaxFailedAttempts)
        {
            _cache.Set($"login_lockout:{normalizedUsername}", true, LockoutDuration);
        }
    }

    public async Task<bool> ChangePasswordAsync(ChangePasswordRequest request)
    {
        var user = await _context.Users.FindAsync(request.UserId);
        if (user == null) return false;

        if (!string.IsNullOrEmpty(request.CurrentPassword) && !VerifyPassword(request.CurrentPassword, user.PasswordHash))
        {
            return false;
        }

        user.PasswordHash = HashPassword(request.NewPassword);
        user.MustChangePassword = false;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        await _activityLog.LogAsync("password_change", $"User '{user.Username}' changed password.", "User", user.Id, user.Id, user.Username);
        if (_securityAudit != null)
            await _securityAudit.RecordAsync("authentication", "password_changed", user.Id, user.Username);
        return true;
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        return await _context.Users
            .Include(u => u.School)
            .Include(u => u.District)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
        catch
        {
            return false;
        }
    }
}

public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;
    private readonly IAuthService _authService;
    private readonly IActivityLogService _activityLog;
    private readonly IPermissionService? _permissions;

    public UserService(ApplicationDbContext context, IAuthService authService, IActivityLogService activityLog, IPermissionService? permissions = null)
    {
        _context = context;
        _authService = authService;
        _activityLog = activityLog;
        _permissions = permissions;
    }

    public async Task<List<User>> GetAllUsersAsync(string? search = null, UserRole? role = null)
    {
        var query = _context.Users
            .Include(u => u.School)
            .Include(u => u.District)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(u => u.Username.ToLower().Contains(term) || u.Name.ToLower().Contains(term) || (u.Email != null && u.Email.ToLower().Contains(term)));
        }

        if (role.HasValue)
        {
            query = query.Where(u => u.Role == role.Value);
        }

        return await query.OrderBy(u => u.Username).ToListAsync();
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        return await _context.Users
            .Include(u => u.School)
            .Include(u => u.District)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User> SaveUserAsync(User user, string? plainPasswordToSet = null)
    {
        await EnsureUserManagementPermissionAsync();
        if (user.Id == 0)
        {
            if (string.IsNullOrWhiteSpace(plainPasswordToSet))
            {
                throw new InvalidOperationException("A password is required when creating a new user.");
            }

            user.PasswordHash = _authService.HashPassword(plainPasswordToSet);
            _context.Users.Add(user);
        }
        else
        {
            var existing = await _context.Users.FindAsync(user.Id);
            if (existing != null)
            {
                existing.Name = user.Name;
                existing.Email = user.Email;
                existing.Role = user.Role;
                existing.SchoolId = user.SchoolId;
                existing.DistrictId = user.DistrictId;
                existing.IsActive = user.IsActive;
                existing.MustChangePassword = user.MustChangePassword;
                existing.UpdatedAt = DateTime.UtcNow;

                if (!string.IsNullOrWhiteSpace(plainPasswordToSet))
                {
                    existing.PasswordHash = _authService.HashPassword(plainPasswordToSet);
                }
                user = existing;
            }
        }

        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<bool> ToggleUserStatusAsync(int id)
    {
        await EnsureUserManagementPermissionAsync();
        var user = await _context.Users.FindAsync(id);
        if (user == null) return false;

        user.IsActive = !user.IsActive;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        await _activityLog.LogAsync("user_toggle", $"Toggled user '{user.Username}' active status to {user.IsActive}.", "User", user.Id);
        return true;
    }

    public async Task<bool> ResetPasswordAsync(int id, string newPassword)
    {
        await EnsureUserManagementPermissionAsync();
        var user = await _context.Users.FindAsync(id);
        if (user == null) return false;

        user.PasswordHash = _authService.HashPassword(newPassword);
        user.MustChangePassword = true;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        await _activityLog.LogAsync("password_reset", $"Reset password for user '{user.Username}'.", "User", user.Id);
        return true;
    }

    private async Task EnsureUserManagementPermissionAsync()
    {
        if (_permissions != null && !await _permissions.HasPermissionAsync(Permissions.UsersManage))
            throw new UnauthorizedAccessException("You do not have permission to manage users.");
    }
}

public class ActivityLogService : IActivityLogService
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantContext? _tenantContext;

    public ActivityLogService(ApplicationDbContext context, ITenantContext? tenantContext = null)
    {
        _context = context;
        _tenantContext = tenantContext;
    }

    public async Task LogAsync(string logName, string description, string? subjectType = null, int? subjectId = null, int? userId = null, string? username = null, object? properties = null)
    {
        try
        {
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

            var log = new ActivityLog
            {
                TenantId = tenantId.Value,
                LogName = logName,
                Description = description,
                SubjectType = subjectType,
                SubjectId = subjectId,
                UserId = userId,
                CausedByUsername = username ?? "System",
                PropertiesJson = properties != null ? JsonSerializer.Serialize(properties) : null,
                CreatedAt = DateTime.UtcNow
            };
            _context.ActivityLogs.Add(log);
            await _context.SaveChangesAsync();
        }
        catch
        {
            // Ignore logging failures to not disrupt primary operations
            foreach (var entry in _context.ChangeTracker.Entries<ActivityLog>().Where(entry => entry.State == EntityState.Added))
            {
                entry.State = EntityState.Detached;
            }
        }
    }

    public async Task<List<ActivityLog>> GetRecentLogsAsync(int limit = 50)
    {
        return await _context.ActivityLogs
            .OrderByDescending(l => l.CreatedAt)
            .Take(limit)
            .ToListAsync();
    }
}
