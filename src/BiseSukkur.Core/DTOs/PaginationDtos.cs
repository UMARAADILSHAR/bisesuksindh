namespace BiseSukkur.Core.DTOs;

public class PagedRequest
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
    public string? SearchTerm { get; set; }
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; } = false;
    public Dictionary<string, string> Filters { get; set; } = new();

    public int Skip => Math.Max(0, (PageNumber - 1) * PageSize);
    public int Take => Math.Clamp(PageSize, 1, 100);
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;

    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    public PagedResult() { }

    public PagedResult(List<T> items, int totalCount, int pageNumber, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }
}

public class EnrollmentMetricsDto
{
    public int TotalEnrolled { get; set; }
    public int SscCount { get; set; }
    public int HscCount { get; set; }
    public int AllottedCount { get; set; }
    public int ChallanedPendingCount { get; set; }
    public int UnInvoicedCount { get; set; }
}

public class ExaminationMetricsDto
{
    public int TotalForms { get; set; }
    public int Part1Count { get; set; }
    public int Part2Count { get; set; }
    public int SlipsIssued { get; set; }
    public int PendingSlips { get; set; }
    public int VerifiedCount { get; set; }
}

public class InvoiceMetricsDto
{
    public int TotalInvoices { get; set; }
    public decimal TotalAmount { get; set; }
    public int EnrollmentCount { get; set; }
    public decimal EnrollmentAmount { get; set; }
    public int ExamCount { get; set; }
    public decimal ExamAmount { get; set; }
    public int VerifiedCount { get; set; }
    public decimal VerifiedAmount { get; set; }
    public int PendingCount { get; set; }
    public decimal PendingAmount { get; set; }
}
