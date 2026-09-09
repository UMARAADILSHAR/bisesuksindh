using System.Diagnostics;
using BiseSukkur.Core.Interfaces;
using BiseSukkur.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BiseSukkur.Infrastructure.Services;

public class HealthDiagnosticsService : IHealthDiagnosticsService
{
    private readonly ApplicationDbContext _context;

    public HealthDiagnosticsService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<HealthDiagnosticsDto> RunDiagnosticsAsync(CancellationToken cancellationToken = default)
    {
        var dto = new HealthDiagnosticsDto();

        try
        {
            var sw = Stopwatch.StartNew();
            dto.DatabaseHealthy = await _context.Database.CanConnectAsync(cancellationToken);

            dto.SchoolCount = await _context.Schools.CountAsync(cancellationToken);
            dto.EnrollmentCount = await _context.Enrollments.CountAsync(cancellationToken);
            dto.ExamCount = await _context.ExaminationForms.CountAsync(cancellationToken);
            dto.InvoiceCount = await _context.Invoices.CountAsync(cancellationToken);
            dto.TotalRecordsCount = dto.SchoolCount + dto.EnrollmentCount + dto.ExamCount + dto.InvoiceCount;

            sw.Stop();
            dto.DatabaseLatencyMs = sw.ElapsedMilliseconds;

            var conn = _context.Database.GetDbConnection();
            dto.DatabaseName = conn.Database;
            dto.DatabaseDataSource = conn.DataSource;
            dto.DatabaseProvider = _context.Database.ProviderName?.Split('.').LastOrDefault() ?? "SQL Server";
        }
        catch (Exception ex)
        {
            dto.DatabaseHealthy = false;
            dto.ErrorMessage = ex.Message;
        }

        return dto;
    }
}
