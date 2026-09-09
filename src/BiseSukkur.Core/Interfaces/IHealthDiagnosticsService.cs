namespace BiseSukkur.Core.Interfaces;

public class HealthDiagnosticsDto
{
    public bool DatabaseHealthy { get; set; }
    public long DatabaseLatencyMs { get; set; }
    public string DatabaseName { get; set; } = string.Empty;
    public string DatabaseDataSource { get; set; } = string.Empty;
    public string DatabaseProvider { get; set; } = string.Empty;
    public int SchoolCount { get; set; }
    public int EnrollmentCount { get; set; }
    public int ExamCount { get; set; }
    public int InvoiceCount { get; set; }
    public int TotalRecordsCount { get; set; }
    public string? ErrorMessage { get; set; }
}

public interface IHealthDiagnosticsService
{
    Task<HealthDiagnosticsDto> RunDiagnosticsAsync(CancellationToken cancellationToken = default);
}
