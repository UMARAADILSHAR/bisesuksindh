using BiseSukkur.Application.Commands.Enrollment;
using BiseSukkur.Application.Validators;
using BiseSukkur.Core.DTOs;
using BiseSukkur.Core.Enums;
using BiseSukkur.Core.Interfaces;
using BiseSukkur.Infrastructure.Data;
using BiseSukkur.Infrastructure.Services;
using BiseSukkur.Web.Authentication;
using BiseSukkur.Web.Authorization;
using BiseSukkur.Web.Components;
using BiseSukkur.Web.Middleware;
using BiseSukkur.Web.Services;
using BiseSukkur.Web.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using MudBlazor.Services;
using System.Security.Claims;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    connectionString = "Server=(localdb)\\mssqllocaldb;Database=BiseSukkurDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();
builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight;
    config.SnackbarConfiguration.PreventDuplicates = false;
    config.SnackbarConfiguration.NewestOnTop = true;
    config.SnackbarConfiguration.ShowCloseIcon = true;
    config.SnackbarConfiguration.VisibleStateDuration = 5000;
    config.SnackbarConfiguration.HideTransitionDuration = 200;
    config.SnackbarConfiguration.ShowTransitionDuration = 200;
    config.SnackbarConfiguration.SnackbarVariant = Variant.Filled;
});

builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<ITenantContext, TenantContext>();
builder.Services.AddScoped<IIpRateLimitService, IpRateLimitService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IActivityLogService, ActivityLogService>();
builder.Services.AddScoped<IWindowService, WindowService>();
builder.Services.AddScoped<IFeeRateService, FeeRateService>();
builder.Services.AddScoped<IEnrollmentNumberService, EnrollmentNumberService>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services.AddScoped<IChallanService, ChallanService>();
builder.Services.AddScoped<IExaminationService, ExaminationService>();
builder.Services.AddScoped<ICertificateService, CertificateService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<ISchoolService, SchoolService>();
builder.Services.AddScoped<IPdfReportService, PdfReportService>();
builder.Services.AddScoped<IPdfAccessService, PdfAccessService>();
builder.Services.AddScoped<IHealthDiagnosticsService, HealthDiagnosticsService>();
builder.Services.AddScoped<IToastNotificationService, ToastNotificationService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IMfaService, MfaService>();
builder.Services.AddScoped<IApprovalService, ApprovalService>();
builder.Services.AddScoped<ISecurityAuditService, SecurityAuditService>();

// MediatR & FluentValidation
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateEnrollmentCommand).Assembly));
builder.Services.AddValidatorsFromAssemblyContaining<CreateEnrollmentValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();

// Resource Authorization Handlers
builder.Services.AddScoped<IAuthorizationHandler, EnrollmentAuthorizationHandler>();
builder.Services.AddScoped<IAuthorizationHandler, InvoiceAuthorizationHandler>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.Cookie.Name = "BiseSukkur.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<CustomAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<CustomAuthenticationStateProvider>());
builder.Services.AddScoped<ICookieSignInService, CookieSignInService>();

builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>("database");

builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("pdf-download", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.User.Identity?.Name ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 30,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));

    options.AddPolicy("login-attempt", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(5),
                QueueLimit = 0
            }));

    options.AddPolicy("certificate-verify", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 20,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
});

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents(options =>
    {
        options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(10);
        options.DisconnectedCircuitMaxRetained = 500;
    });

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.MigrateAsync();
    await DbInitializer.SeedAsync(dbContext, seedPassword: builder.Configuration["SeedPassword"], isDevelopment: app.Environment.IsDevelopment());
}

// Forward headers from ALB / reverse proxy (X-Forwarded-For, X-Forwarded-Proto)
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseSecurityHeaders();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseGlobalExceptionHandling();

app.UseRateLimiter();
app.UseAntiforgery();
app.UseStaticFiles();
app.MapStaticAssets();

app.UseAuthentication();
app.UseAuthorization();

// Liveness probe — always returns 200 (no dependency checks)
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false // exclude all checks, just returns Healthy
});

// Readiness probe — checks database connectivity
app.MapHealthChecks("/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready") || check.Name == "database"
});

var pdfRoutes = app.MapGroup("/api/pdf")
    .RequireAuthorization()
    .RequireRateLimiting("pdf-download");

pdfRoutes.MapGet("/enrollment-card/{id:int}", async (
    int id,
    IPdfReportService pdfService,
    IPdfAccessService accessService,
    ClaimsPrincipal user) =>
{
    if (!await accessService.CanAccessEnrollmentCardAsync(user, id))
    {
        return Results.Forbid();
    }

    var bytes = await pdfService.GenerateEnrollmentCardPdfAsync(id);
    return Results.File(bytes, "application/pdf", $"EnrollmentCard_{id}.pdf");
});

pdfRoutes.MapGet("/roll-slip/{id:int}", async (
    int id,
    IPdfReportService pdfService,
    IPdfAccessService accessService,
    ClaimsPrincipal user) =>
{
    if (!await accessService.CanAccessRollSlipAsync(user, id))
    {
        return Results.Forbid();
    }

    var bytes = await pdfService.GenerateRollNumberSlipPdfAsync(id);
    return Results.File(bytes, "application/pdf", $"RollSlip_{id}.pdf");
});

pdfRoutes.MapGet("/certificate/{id:int}", async (
    int id,
    IPdfReportService pdfService,
    IPdfAccessService accessService,
    ClaimsPrincipal user) =>
{
    if (!await accessService.CanAccessCertificateAsync(user, id))
    {
        return Results.Forbid();
    }

    var bytes = await pdfService.GenerateCertificatePdfAsync(id);
    return Results.File(bytes, "application/pdf", $"Certificate_{id}.pdf");
});

pdfRoutes.MapGet("/bank-challan/{id:int}", async (
    int id,
    IPdfReportService pdfService,
    IPdfAccessService accessService,
    ClaimsPrincipal user) =>
{
    if (!await accessService.CanAccessInvoicePdfAsync(user, id))
    {
        return Results.Forbid();
    }

    var bytes = await pdfService.GenerateBankChallanPdfAsync(id);
    return Results.File(bytes, "application/pdf", $"BankChallan_{id}.pdf");
});

pdfRoutes.MapGet("/student-list/{id:int}", async (
    int id,
    IPdfReportService pdfService,
    IPdfAccessService accessService,
    ClaimsPrincipal user) =>
{
    if (!await accessService.CanAccessInvoicePdfAsync(user, id))
    {
        return Results.Forbid();
    }

    var bytes = await pdfService.GenerateCandidateListPdfAsync(id);
    return Results.File(bytes, "application/pdf", $"CandidateList_{id}.pdf");
});

// Standard HTTP Cookie Authentication Endpoints
app.MapPost("/api/auth/login", async (
    HttpContext httpContext,
    IAuthService authService,
    [FromForm] string username,
    [FromForm] string password,
    [FromForm] string? mfaCode,
    [FromForm] string? returnUrl) =>
{
    var response = await authService.LoginAsync(new LoginRequest { Username = username, Password = password, MfaCode = mfaCode });
    if (!response.Success || response.User == null)
    {
        var error = Uri.EscapeDataString(response.ErrorMessage ?? "Invalid username or password.");
        var mfa = response.MfaRequired ? "&mfa=1" : string.Empty;
        return Results.Redirect($"/login?error={error}{mfa}");
    }

    var identity = new ClaimsIdentity(response.User.ToClaims(), CookieAuthenticationDefaults.AuthenticationScheme);
    var principal = new ClaimsPrincipal(identity);
    var authProps = new AuthenticationProperties
    {
        IsPersistent = true,
        AllowRefresh = true,
        ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
    };

    await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProps);

    if (response.MustChangePassword)
    {
        return Results.Redirect("/auth/forced-password-change");
    }

    if (!string.IsNullOrEmpty(returnUrl) && returnUrl.StartsWith("/"))
    {
        return Results.Redirect(returnUrl);
    }

    return response.User.Role switch
    {
        UserRole.SuperAdmin => Results.Redirect("/superadmin/dashboard"),
        UserRole.DistrictAdmin => Results.Redirect("/district/dashboard"),
        UserRole.SchoolAdmin => Results.Redirect("/school/dashboard"),
        _ => Results.Redirect("/login")
    };
});

app.MapPost("/api/auth/logout", async (HttpContext httpContext) =>
{
    await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/login");
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

public partial class Program;
