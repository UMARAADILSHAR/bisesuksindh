using BiseSukkur.Core.Entities;
using BiseSukkur.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BiseSukkur.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    private readonly ITenantContext? _tenantContext;
    private int? _seedTenantId;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ITenantContext? tenantContext = null) : base(options)
    {
        _tenantContext = tenantContext;
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<User> Users => Set<User>();
    public DbSet<District> Districts => Set<District>();
    public DbSet<Tehsil> Tehsils => Set<Tehsil>();
    public DbSet<School> Schools => Set<School>();
    public DbSet<SchoolSpecialPermission> SchoolSpecialPermissions => Set<SchoolSpecialPermission>();
    public DbSet<AcademicYear> AcademicYears => Set<AcademicYear>();
    public DbSet<WindowOverride> WindowOverrides => Set<WindowOverride>();
    public DbSet<FeeRate> FeeRates => Set<FeeRate>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceItem> InvoiceItems => Set<InvoiceItem>();
    public DbSet<InvoiceSequence> InvoiceSequences => Set<InvoiceSequence>();
    public DbSet<ExamCenter> ExamCenters => Set<ExamCenter>();
    public DbSet<ExamCenterSchool> ExamCenterSchools => Set<ExamCenterSchool>();
    public DbSet<ExamSchedule> ExamSchedules => Set<ExamSchedule>();
    public DbSet<ExaminationForm> ExaminationForms => Set<ExaminationForm>();
    public DbSet<Result> Results => Set<Result>();
    public DbSet<Certificate> Certificates => Set<Certificate>();
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();
    public DbSet<Setting> Settings => Set<Setting>();
    public DbSet<PermissionGrant> PermissionGrants => Set<PermissionGrant>();
    public DbSet<ApprovalRequest> ApprovalRequests => Set<ApprovalRequest>();
    public DbSet<SecurityAuditEvent> SecurityAuditEvents => Set<SecurityAuditEvent>();
    public DbSet<ExaminationSession> ExaminationSessions => Set<ExaminationSession>();
    public DbSet<SubjectScheme> SubjectSchemes => Set<SubjectScheme>();
    public DbSet<SchoolAffiliationApplication> SchoolAffiliationApplications => Set<SchoolAffiliationApplication>();
    public DbSet<SchoolInspection> SchoolInspections => Set<SchoolInspection>();
    public DbSet<ResultRecheckRequest> ResultRecheckRequests => Set<ResultRecheckRequest>();
    public DbSet<ResultCorrectionRequest> ResultCorrectionRequests => Set<ResultCorrectionRequest>();
    public DbSet<CertificateStatusHistory> CertificateStatusHistories => Set<CertificateStatusHistory>();

    public int? CurrentTenantId => _seedTenantId ?? _tenantContext?.TenantId;
    public bool IsPlatformAdministrator => _tenantContext == null || _tenantContext.IsPlatformAdministrator;

    public void SetTenantForSeeding(int tenantId)
    {
        _seedTenantId = tenantId;
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ApplyTenantIdsToAddedEntities();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        ApplyTenantIdsToAddedEntities();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void ApplyTenantIdsToAddedEntities()
    {
        if (!CurrentTenantId.HasValue)
        {
            return;
        }

        foreach (var entry in ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added && e.Entity is not Tenant))
        {
            var tenantProperty = entry.Metadata.FindProperty(nameof(User.TenantId));
            if (tenantProperty != null && entry.Property(nameof(User.TenantId)).CurrentValue is 0)
            {
                entry.Property(nameof(User.TenantId)).CurrentValue = CurrentTenantId.Value;
            }
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.Property(t => t.Name).HasMaxLength(200).IsRequired();
            entity.Property(t => t.Code).HasMaxLength(50).IsRequired();
            entity.HasIndex(t => t.Code).IsUnique();
        });

        foreach (var entityType in modelBuilder.Model.GetEntityTypes()
            .Where(t => t.ClrType != typeof(Tenant) && t.FindProperty(nameof(User.TenantId)) != null))
        {
            modelBuilder.Entity(entityType.ClrType)
                .HasOne(typeof(Tenant), "Tenant")
                .WithMany()
                .HasForeignKey(nameof(User.TenantId))
                .OnDelete(DeleteBehavior.Restrict);

            var parameter = Expression.Parameter(entityType.ClrType, "entity");
            var tenantId = Expression.Call(
                typeof(EF),
                nameof(EF.Property),
                new[] { typeof(int) },
                parameter,
                Expression.Constant(nameof(User.TenantId)));
            var currentTenantId = Expression.Property(
                Expression.Constant(this),
                nameof(CurrentTenantId));
            var tenantMatches = Expression.Equal(
                Expression.Convert(tenantId, typeof(int?)),
                currentTenantId);
            var platformBypass = Expression.Property(
                Expression.Constant(this),
                nameof(IsPlatformAdministrator));
            var filter = Expression.Lambda(
                Expression.OrElse(platformBypass, tenantMatches),
                parameter);

            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
        }

        // Configure Decimal precision for SQL Server
        modelBuilder.Entity<AcademicYear>(entity =>
        {
            entity.Property(a => a.EnrollmentLateFeeAmount).HasColumnType("decimal(18,2)");
            entity.Property(a => a.ExamLateFeeAmount).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<WindowOverride>(entity =>
        {
            entity.Property(w => w.LateFeeAmount).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<FeeRate>(entity =>
        {
            entity.Property(f => f.StandardFee).HasColumnType("decimal(18,2)");
            entity.Property(f => f.LateFee).HasColumnType("decimal(18,2)");
            entity.HasOne(fr => fr.AcademicYear)
                .WithMany(ay => ay.FeeRates)
                .HasForeignKey(fr => fr.AcademicYearId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PermissionGrant>(entity =>
        {
            entity.Property(p => p.Permission).HasMaxLength(100).IsRequired();
            entity.HasIndex(p => new { p.UserId, p.Permission, p.ScopeType, p.ScopeId }).IsUnique();
            entity.HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ApprovalRequest>(entity =>
        {
            entity.Property(a => a.RequestType).HasMaxLength(100).IsRequired();
            entity.Property(a => a.EntityType).HasMaxLength(100).IsRequired();
            entity.Property(a => a.EntityId).HasMaxLength(100).IsRequired();
            entity.Property(a => a.SubmittedByUsername).HasMaxLength(100).IsRequired();
            entity.Property(a => a.ReviewedByUsername).HasMaxLength(100);
            entity.HasIndex(a => new { a.EntityType, a.EntityId, a.Status });
        });

        modelBuilder.Entity<SecurityAuditEvent>(entity =>
        {
            entity.Property(a => a.EventType).HasMaxLength(100).IsRequired();
            entity.Property(a => a.Action).HasMaxLength(100).IsRequired();
            entity.Property(a => a.Username).HasMaxLength(100);
            entity.Property(a => a.IpAddress).HasMaxLength(64);
            entity.Property(a => a.CorrelationId).HasMaxLength(100);
            entity.HasIndex(a => new { a.OccurredAt, a.EventType });
            entity.HasIndex(a => new { a.EntityType, a.EntityId });
        });

        modelBuilder.Entity<ExaminationSession>(entity =>
        {
            entity.Property(s => s.Code).HasMaxLength(50).IsRequired();
            entity.Property(s => s.Name).HasMaxLength(200).IsRequired();
            entity.HasIndex(s => s.Code).IsUnique();
            entity.HasIndex(s => new { s.AcademicYearId, s.ClassLevel, s.Status });
            entity.HasOne(s => s.AcademicYear)
                .WithMany()
                .HasForeignKey(s => s.AcademicYearId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SubjectScheme>(entity =>
        {
            entity.Property(s => s.SubjectCode).HasMaxLength(50).IsRequired();
            entity.Property(s => s.SubjectName).HasMaxLength(200).IsRequired();
            entity.HasIndex(s => new { s.ExaminationSessionId, s.SubjectCode, s.Group }).IsUnique();
            entity.HasOne(s => s.ExaminationSession)
                .WithMany()
                .HasForeignKey(s => s.ExaminationSessionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SchoolAffiliationApplication>(entity =>
        {
            entity.Property(a => a.ApplicationNumber).HasMaxLength(50).IsRequired();
            entity.HasIndex(a => a.ApplicationNumber).IsUnique();
            entity.HasIndex(a => new { a.SchoolId, a.Status });
            entity.HasOne(a => a.School)
                .WithMany()
                .HasForeignKey(a => a.SchoolId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SchoolInspection>(entity =>
        {
            entity.Property(i => i.InspectorUsername).HasMaxLength(100).IsRequired();
            entity.HasIndex(i => new { i.AffiliationApplicationId, i.InspectedAt });
            entity.HasOne(i => i.AffiliationApplication)
                .WithMany(a => a.Inspections)
                .HasForeignKey(i => i.AffiliationApplicationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ResultRecheckRequest>(entity =>
        {
            entity.Property(r => r.RequestNumber).HasMaxLength(50).IsRequired();
            entity.HasIndex(r => r.RequestNumber).IsUnique();
            entity.HasIndex(r => new { r.ResultId, r.Status });
            entity.HasOne(r => r.Result).WithMany().HasForeignKey(r => r.ResultId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ResultCorrectionRequest>(entity =>
        {
            entity.Property(r => r.RequestNumber).HasMaxLength(50).IsRequired();
            entity.HasIndex(r => r.RequestNumber).IsUnique();
            entity.HasIndex(r => new { r.ResultId, r.Status });
            entity.HasOne(r => r.Result).WithMany().HasForeignKey(r => r.ResultId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CertificateStatusHistory>(entity =>
        {
            entity.Property(h => h.ChangedByUsername).HasMaxLength(100).IsRequired();
            entity.HasIndex(h => new { h.CertificateId, h.ChangedAt });
            entity.HasOne(h => h.Certificate).WithMany().HasForeignKey(h => h.CertificateId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.Property(i => i.Amount).HasColumnType("decimal(18,2)");
            entity.Property(i => i.PaymentReference).HasMaxLength(100);
            entity.Property(i => i.PaymentMethod).HasMaxLength(50);
            entity.Property(i => i.PaymentVerifiedByUsername).HasMaxLength(100);
            entity.HasIndex(i => i.InvoiceNumber).IsUnique();
            entity.HasIndex(i => i.PaymentReference)
                .IsUnique()
                .HasFilter("[PaymentReference] IS NOT NULL");
            entity.HasIndex(i => new { i.SchoolId, i.AcademicYearId, i.Status });
            entity.HasOne(i => i.School)
                .WithMany(s => s.Invoices)
                .HasForeignKey(i => i.SchoolId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(i => i.AcademicYear)
                .WithMany(a => a.Invoices)
                .HasForeignKey(i => i.AcademicYearId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<InvoiceItem>(entity =>
        {
            entity.Property(ii => ii.FeeAmount).HasColumnType("decimal(18,2)");
            entity.HasOne(ii => ii.Invoice)
                .WithMany(i => i.Items)
                .HasForeignKey(ii => ii.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(ii => ii.Enrollment)
                .WithMany(e => e.InvoiceItems)
                .HasForeignKey(ii => ii.EnrollmentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Result>(entity =>
        {
            entity.Property(r => r.Percentage).HasColumnType("decimal(5,2)");
            entity.HasOne(r => r.ExaminationForm)
                .WithOne(ef => ef.Result)
                .HasForeignKey<Result>(r => r.ExaminationFormId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Username).IsUnique();
            entity.HasOne(u => u.School)
                .WithMany(s => s.Users)
                .HasForeignKey(u => u.SchoolId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(u => u.District)
                .WithMany(d => d.Users)
                .HasForeignKey(u => u.DistrictId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // School
        modelBuilder.Entity<School>(entity =>
        {
            entity.HasIndex(s => s.SemisCode).IsUnique();
            entity.HasOne(s => s.District)
                .WithMany(d => d.Schools)
                .HasForeignKey(s => s.DistrictId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(s => s.Tehsil)
                .WithMany(t => t.Schools)
                .HasForeignKey(s => s.TehsilId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Tehsil
        modelBuilder.Entity<Tehsil>(entity =>
        {
            entity.HasOne(t => t.District)
                .WithMany(d => d.Tehsils)
                .HasForeignKey(t => t.DistrictId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Enrollment
        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.HasIndex(e => new { e.SchoolId, e.AcademicYearId, e.Cnic });
            entity.HasIndex(e => e.EnrollmentNumber).IsUnique();
            entity.HasOne(e => e.School)
                .WithMany(s => s.Enrollments)
                .HasForeignKey(e => e.SchoolId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.AcademicYear)
                .WithMany(a => a.Enrollments)
                .HasForeignKey(e => e.AcademicYearId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Invoice)
                .WithMany()
                .HasForeignKey(e => e.InvoiceId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // ExaminationForm
        modelBuilder.Entity<ExaminationForm>(entity =>
        {
            entity.HasOne(ef => ef.Enrollment)
                .WithOne(e => e.ExaminationForm)
                .HasForeignKey<ExaminationForm>(ef => ef.EnrollmentId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(ef => ef.ExamCenter)
                .WithMany(ec => ec.ExaminationForms)
                .HasForeignKey(ef => ef.ExamCenterId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Certificate
        modelBuilder.Entity<Certificate>(entity =>
        {
            entity.HasIndex(c => c.CertificateNumber).IsUnique();
            entity.HasIndex(c => c.VerificationToken).IsUnique();
            entity.HasOne(c => c.Result)
                .WithOne(r => r.Certificate)
                .HasForeignKey<Certificate>(c => c.ResultId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
