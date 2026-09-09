using BiseSukkur.Core.DTOs;
using BiseSukkur.Core.Entities;
using BiseSukkur.Core.Enums;
using BiseSukkur.Core.Interfaces;
using BiseSukkur.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Text.Json;

namespace BiseSukkur.Infrastructure.Services;

public class ChallanService : IChallanService
{
    private readonly ApplicationDbContext _context;
    private readonly IWindowService _windowService;
    private readonly IEnrollmentNumberService _enrollmentNumberService;
    private readonly IActivityLogService _activityLog;

    public ChallanService(
        ApplicationDbContext context,
        IWindowService windowService,
        IEnrollmentNumberService enrollmentNumberService,
        IActivityLogService activityLog)
    {
        _context = context;
        _windowService = windowService;
        _enrollmentNumberService = enrollmentNumberService;
        _activityLog = activityLog;
    }

    public async Task<List<string>> GetEligibleClassesAsync(int schoolId, int academicYearId, string challanType)
    {
        var school = await _context.Schools.FindAsync(schoolId);
        var allowedLevels = new List<string> { "SSC-I", "SSC-II", "HSC-I", "HSC-II" };
        if (school != null && !string.IsNullOrEmpty(school.AllowedLevelsJson))
        {
            try
            {
                allowedLevels = JsonSerializer.Deserialize<List<string>>(school.AllowedLevelsJson) ?? allowedLevels;
            }
            catch { }
        }

        var available = new List<string>();
        foreach (var lvl in allowedLevels)
        {
            var query = BuildEligibleQuery(schoolId, academicYearId, challanType, lvl);
            if (await query.AnyAsync())
            {
                available.Add(lvl);
            }
        }

        return available.Any() ? available : allowedLevels;
    }

    public async Task<List<string>> GetEligibleGroupsAsync(int schoolId, int academicYearId, string challanType, string classLevel)
    {
        var groups = await BuildEligibleQuery(schoolId, academicYearId, challanType, classLevel)
            .Select(e => e.Group)
            .Distinct()
            .OrderBy(g => g)
            .ToListAsync();

        if (groups.Any()) return groups;

        return classLevel.StartsWith("SSC", StringComparison.OrdinalIgnoreCase)
            ? new List<string> { "SCIENCE", "GENERAL REGULAR", "Science", "General Regular" }
            : new List<string> { "PRE-MEDICAL", "PRE-ENGINEERING", "COMMERCE", "HUMANITIES" };
    }

    public async Task<List<string>> GetEligibleStudentTypesAsync(int schoolId, int academicYearId, string challanType, string classLevel, string group)
    {
        var types = await BuildEligibleQuery(schoolId, academicYearId, challanType, classLevel, group)
            .Select(e => e.StudentType)
            .Distinct()
            .OrderBy(st => st)
            .ToListAsync();

        if (types.Any()) return types;

        return new List<string> { "FRESH", "Regular", "Private", "Repeater" };
    }

    public async Task<FeeRateResultDto> GetFeeRateAsync(int schoolId, string challanType, string classLevel, string group, string studentType, int academicYearId)
    {
        var school = await _context.Schools.FindAsync(schoolId);
        if (school == null) return new FeeRateResultDto { ErrorMessage = "School not found." };

        var statusSummary = await _windowService.GetWindowStatusSummaryAsync(challanType, schoolId);
        if (statusSummary.Phase == "closed")
        {
            return new FeeRateResultDto
            {
                Phase = "closed",
                LateFeeApplies = false,
                ErrorMessage = "The submission window is currently closed for this school."
            };
        }

        var slab = school.Type == SchoolType.Private ? "private" : "public";

        var clVariants = GetClassLevelVariants(classLevel);

        var rate = await _context.FeeRates
            .FirstOrDefaultAsync(r =>
                r.AcademicYearId == academicYearId &&
                r.FeeType == challanType &&
                (clVariants.Contains(r.ClassLevel) || r.ClassLevel == classLevel || (classLevel.StartsWith("SSC-I") && r.ClassLevel == "SSC-I") || (classLevel.StartsWith("SSC-II") && r.ClassLevel == "SSC-II") || (classLevel.StartsWith("HSC-I") && r.ClassLevel == "HSC-I") || (classLevel.StartsWith("HSC-II") && r.ClassLevel == "HSC-II")) &&
                (r.GroupName.ToLower() == group.ToLower() || r.GroupName.ToLower().Replace(" ", "_") == group.ToLower().Replace(" ", "_") || r.GroupName.ToLower().Replace(" ", "") == group.ToLower().Replace(" ", "")) &&
                (r.StudentType.ToLower() == studentType.ToLower() || (studentType.ToLower() == "regular" && (r.StudentType.ToLower() == "fresh" || r.StudentType.ToLower() == "regular")) || (studentType.ToLower() == "fresh" && (r.StudentType.ToLower() == "regular" || r.StudentType.ToLower() == "fresh"))) &&
                r.FeeSlab == slab &&
                r.IsActive);

        if (rate == null)
        {
            // Fallback general lookup if exact group mismatch
            rate = await _context.FeeRates
                .FirstOrDefaultAsync(r =>
                    r.AcademicYearId == academicYearId &&
                    r.FeeType == challanType &&
                    (clVariants.Contains(r.ClassLevel) || (classLevel.StartsWith("SSC-I") && r.ClassLevel == "SSC-I") || (classLevel.StartsWith("SSC-II") && r.ClassLevel == "SSC-II") || (classLevel.StartsWith("HSC-I") && r.ClassLevel == "HSC-I") || (classLevel.StartsWith("HSC-II") && r.ClassLevel == "HSC-II")) &&
                    r.FeeSlab == slab &&
                    r.IsActive);
        }

        if (rate == null)
        {
            return new FeeRateResultDto
            {
                Phase = statusSummary.Phase,
                ErrorMessage = "Fee rate not configured for this class/group combination. Please contact the Board Admin."
            };
        }

        var standardFee = rate.StandardFee;
        var lateFeeApplies = statusSummary.Phase == "grace";
        decimal feeAmount = standardFee;

        if (lateFeeApplies)
        {
            var timeline = await _windowService.ResolveTimelineAsync(schoolId, challanType);
            if (timeline.LateFeeType == "additional" && timeline.LateFeeAmount > 0)
            {
                feeAmount = standardFee + timeline.LateFeeAmount;
            }
            else if (rate.LateFee > 0)
            {
                feeAmount = rate.LateFee;
            }
            else if (timeline.LateFeeAmount > 0)
            {
                feeAmount = timeline.LateFeeAmount;
            }
        }

        return new FeeRateResultDto
        {
            FeeAmount = feeAmount,
            StandardFee = standardFee,
            Phase = statusSummary.Phase,
            LateFeeApplies = lateFeeApplies
        };
    }

    public async Task<List<StudentSelectionDto>> GetEligibleStudentsAsync(int schoolId, int academicYearId, string challanType, string classLevel, string group, string studentType)
    {
        var students = await BuildEligibleQuery(schoolId, academicYearId, challanType, classLevel, group, studentType)
            .OrderBy(e => e.Id)
            .ToListAsync();

        var combinationKey = $"{classLevel}-{group}-{studentType}";

        var result = new List<StudentSelectionDto>();
        foreach (var s in students)
        {
            bool wasExcluded = await _context.InvoiceItems
                .Include(ii => ii.Invoice)
                .AnyAsync(ii =>
                    ii.EnrollmentId == s.Id &&
                    !ii.IsIncluded &&
                    ii.Invoice != null &&
                    ii.Invoice.InvoiceType == challanType &&
                    ii.Invoice.AcademicYearId == academicYearId &&
                    ii.Invoice.ClassGroupStudentType == combinationKey &&
                    ii.Invoice.Status != InvoiceStatus.Rejected &&
                    ii.Invoice.Status != InvoiceStatus.Cancelled);

            result.Add(new StudentSelectionDto
            {
                Id = s.Id,
                StudentName = s.StudentName.ToUpperInvariant(),
                FatherName = s.FatherName?.ToUpperInvariant(),
                Surname = s.Surname?.ToUpperInvariant(),
                GrNumber = s.GrNumber,
                Cnic = s.Cnic,
                EnrollmentNumber = s.EnrollmentNumber,
                ClassLevel = s.ClassLevel,
                Group = s.Group,
                StudentType = s.StudentType,
                IsIncluded = true,
                PreviouslyExcluded = wasExcluded
            });
        }
        return result;
    }

    public async Task<ChallanGenerationResultDto> GenerateChallanAsync(ChallanGenerationRequestDto request)
    {
        var school = await _context.Schools.FindAsync(request.SchoolId);
        if (school == null) return new ChallanGenerationResultDto { Success = false, ErrorMessage = "School not found." };

        var feeData = await GetFeeRateAsync(request.SchoolId, request.ChallanType, request.ClassLevel, request.Group, request.StudentType, request.AcademicYearId);
        if (feeData.FeeAmount == null)
        {
            return new ChallanGenerationResultDto { Success = false, ErrorMessage = feeData.ErrorMessage ?? "Unable to calculate fee." };
        }

        var includedStudents = request.Students.Where(s => s.IsIncluded).ToList();
        if (!includedStudents.Any())
        {
            return new ChallanGenerationResultDto { Success = false, ErrorMessage = "Please select at least one student to include in the challan." };
        }

        var feePerStudent = feeData.FeeAmount.Value;
        var includedIds = includedStudents.Select(s => s.Id).ToList();

        // Check if any student already has an active invoice
        var activeExisting = await _context.InvoiceItems
            .Include(ii => ii.Invoice)
            .Include(ii => ii.Enrollment)
            .Where(ii =>
                includedIds.Contains(ii.EnrollmentId) &&
                ii.IsIncluded &&
                ii.Invoice != null &&
                ii.Invoice.SchoolId == request.SchoolId &&
                ii.Invoice.AcademicYearId == request.AcademicYearId &&
                ii.Invoice.InvoiceType == request.ChallanType &&
                ii.Invoice.Status != InvoiceStatus.Rejected &&
                ii.Invoice.Status != InvoiceStatus.Cancelled)
            .Select(ii => ii.Enrollment != null ? ii.Enrollment.StudentName : $"ID {ii.EnrollmentId}")
            .ToListAsync();

        if (activeExisting.Any())
        {
            return new ChallanGenerationResultDto
            {
                Success = false,
                ErrorMessage = $"These students are already on an active {request.ChallanType} challan: {string.Join(", ", activeExisting)}."
            };
        }

        // Generate unique Invoice Number e.g. "KH1-001-001"
        var invoiceSequence = await _context.InvoiceSequences.FirstOrDefaultAsync(s => s.SequenceType == "invoice");
        if (invoiceSequence == null)
        {
            invoiceSequence = new InvoiceSequence { SequenceType = "invoice", LastNumber = 0 };
            _context.InvoiceSequences.Add(invoiceSequence);
            await _context.SaveChangesAsync();
        }

        invoiceSequence.LastNumber += 1;
        invoiceSequence.UpdatedAt = DateTime.UtcNow;

        var usernamePrefix = !string.IsNullOrEmpty(request.Username) ? request.Username.ToUpper() : "SCH";
        var invoiceNumber = $"{usernamePrefix}-{invoiceSequence.LastNumber:D3}";

        var combinationKey = $"{request.ClassLevel}-{request.Group}-{request.StudentType}";
        var totalAmount = includedStudents.Count * feePerStudent;

        var invoice = new Invoice
        {
            SchoolId = request.SchoolId,
            AcademicYearId = request.AcademicYearId,
            InvoiceType = request.ChallanType,
            InvoiceNumber = invoiceNumber,
            ClassGroupStudentType = combinationKey,
            StudentCount = includedStudents.Count,
            Amount = totalAmount,
            Status = InvoiceStatus.Pending,
            Phase = feeData.Phase,
            GeneratedAt = DateTime.UtcNow
        };

        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();

        foreach (var s in request.Students)
        {
            var item = new InvoiceItem
            {
                InvoiceId = invoice.Id,
                EnrollmentId = s.Id,
                IsIncluded = s.IsIncluded,
                FeeAmount = s.IsIncluded ? feePerStudent : 0m,
                InvoiceTypeSnapshot = request.ChallanType
            };
            _context.InvoiceItems.Add(item);

            if (s.IsIncluded && request.ChallanType == "enrollment")
            {
                var enrollment = await _context.Enrollments.FindAsync(s.Id);
                if (enrollment != null)
                {
                    enrollment.InvoiceId = invoice.Id;
                    enrollment.ChallanStatus = "generated";
                }
            }
        }

        await _context.SaveChangesAsync();

        await _activityLog.LogAsync("challan_generation",
            $"Generated {request.ChallanType} challan #{invoiceNumber} for {includedStudents.Count} students (Amount: Rs. {totalAmount:N0}).",
            "Invoice", invoice.Id, null, request.Username);

        return new ChallanGenerationResultDto
        {
            Success = true,
            InvoiceId = invoice.Id,
            InvoiceUuid = invoice.Uuid,
            InvoiceNumber = invoice.InvoiceNumber,
            IncludedCount = includedStudents.Count,
            ExcludedCount = request.Students.Count - includedStudents.Count,
            TotalAmount = totalAmount,
            FeePerStudent = feePerStudent,
            Phase = feeData.Phase
        };
    }

    public async Task<Invoice?> GetInvoiceByIdAsync(int id)
    {
        return await _context.Invoices
            .Include(i => i.School).ThenInclude(s => s!.District)
            .Include(i => i.AcademicYear)
            .Include(i => i.Items).ThenInclude(it => it.Enrollment)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<Invoice?> GetInvoiceByUuidAsync(Guid uuid)
    {
        return await _context.Invoices
            .Include(i => i.School).ThenInclude(s => s!.District)
            .Include(i => i.AcademicYear)
            .Include(i => i.Items).ThenInclude(it => it.Enrollment)
            .FirstOrDefaultAsync(i => i.Uuid == uuid);
    }

    public async Task<List<Invoice>> GetInvoicesBySchoolAsync(int schoolId, int? academicYearId = null)
    {
        var query = _context.Invoices
            .Include(i => i.AcademicYear)
            .Where(i => i.SchoolId == schoolId);

        if (academicYearId.HasValue)
        {
            query = query.Where(i => i.AcademicYearId == academicYearId.Value);
        }

        return await query.OrderByDescending(i => i.GeneratedAt).ToListAsync();
    }

    public async Task<PagedResult<Invoice>> GetPagedInvoicesAsync(int schoolId, PagedRequest request)
    {
        var query = _context.Invoices
            .AsNoTracking()
            .Include(i => i.AcademicYear)
            .Where(i => i.SchoolId == schoolId);

        if (request.Filters.TryGetValue("academicYearId", out var yearStr) && int.TryParse(yearStr, out var yearId))
        {
            query = query.Where(i => i.AcademicYearId == yearId);
        }

        if (request.Filters.TryGetValue("status", out var statusStr) && !string.IsNullOrWhiteSpace(statusStr)
            && Enum.TryParse<InvoiceStatus>(statusStr, true, out var statusValue))
        {
            query = query.Where(i => i.Status == statusValue);
        }

        if (request.Filters.TryGetValue("invoiceType", out var typeFilter) && !string.IsNullOrWhiteSpace(typeFilter) && typeFilter != "all")
        {
            query = query.Where(i => i.InvoiceType == typeFilter);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim().ToLower();
            query = query.Where(i =>
                i.InvoiceNumber.ToLower().Contains(term)
                || (i.ClassGroupStudentType != null && i.ClassGroupStudentType.ToLower().Contains(term)));
        }

        var totalCount = await query.CountAsync();

        query = request.SortBy?.ToLower() switch
        {
            "amount" => request.SortDescending ? query.OrderByDescending(i => i.Amount) : query.OrderBy(i => i.Amount),
            "generatedat" => request.SortDescending ? query.OrderByDescending(i => i.GeneratedAt) : query.OrderBy(i => i.GeneratedAt),
            "invoicenumber" => request.SortDescending ? query.OrderByDescending(i => i.InvoiceNumber) : query.OrderBy(i => i.InvoiceNumber),
            _ => request.SortDescending ? query.OrderByDescending(i => i.Id) : query.OrderBy(i => i.Id)
        };

        var items = await query.Skip(request.Skip).Take(request.Take).ToListAsync();
        return new PagedResult<Invoice>(items, totalCount, request.PageNumber, request.Take);
    }

    public async Task<InvoiceMetricsDto> GetInvoiceMetricsAsync(int schoolId, int? academicYearId = null)
    {
        var query = _context.Invoices.Where(i => i.SchoolId == schoolId);
        if (academicYearId.HasValue)
        {
            query = query.Where(i => i.AcademicYearId == academicYearId.Value);
        }

        var invoices = await query.ToListAsync();

        return new InvoiceMetricsDto
        {
            TotalInvoices = invoices.Count,
            TotalAmount = invoices.Sum(i => i.Amount),
            EnrollmentCount = invoices.Count(i => i.InvoiceType == "enrollment"),
            EnrollmentAmount = invoices.Where(i => i.InvoiceType == "enrollment").Sum(i => i.Amount),
            ExamCount = invoices.Count(i => i.InvoiceType == "exam"),
            ExamAmount = invoices.Where(i => i.InvoiceType == "exam").Sum(i => i.Amount),
            VerifiedCount = invoices.Count(i => i.Status == InvoiceStatus.Verified),
            VerifiedAmount = invoices.Where(i => i.Status == InvoiceStatus.Verified).Sum(i => i.Amount),
            PendingCount = invoices.Count(i => i.Status == InvoiceStatus.Pending),
            PendingAmount = invoices.Where(i => i.Status == InvoiceStatus.Pending).Sum(i => i.Amount)
        };
    }

    public async Task<int> GetPendingInvoiceCountAsync(int schoolId, int? academicYearId = null)
    {
        var query = _context.Invoices.Where(i => i.SchoolId == schoolId && i.Status == InvoiceStatus.Pending);
        if (academicYearId.HasValue)
        {
            query = query.Where(i => i.AcademicYearId == academicYearId.Value);
        }
        return await query.CountAsync();
    }

    public async Task<List<Invoice>> GetAllInvoicesAsync(string? search = null, InvoiceStatus? status = null, int? academicYearId = null)
    {
        var query = _context.Invoices
            .Include(i => i.School).ThenInclude(s => s!.District)
            .Include(i => i.AcademicYear)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(i => i.InvoiceNumber.ToLower().Contains(term) || (i.School != null && i.School.Name.ToLower().Contains(term)));
        }

        if (status.HasValue)
        {
            query = query.Where(i => i.Status == status.Value);
        }

        if (academicYearId.HasValue)
        {
            query = query.Where(i => i.AcademicYearId == academicYearId.Value);
        }

        return await query.OrderByDescending(i => i.GeneratedAt).ToListAsync();
    }

    public async Task<PaymentVerificationResultDto> VerifyAndMarkAsPaidAsync(
        int invoiceId,
        PaymentVerificationRequestDto payment,
        string verifiedByUsername)
    {
        if (invoiceId <= 0)
        {
            return PaymentVerificationResultDto.Failed("A valid challan invoice is required.");
        }

        var paymentReference = payment.PaymentReference?.Trim();
        if (string.IsNullOrWhiteSpace(paymentReference))
        {
            return PaymentVerificationResultDto.Failed("Enter the bank transaction or receipt reference before verifying payment.");
        }

        if (paymentReference.Length > 100)
        {
            return PaymentVerificationResultDto.Failed("The payment reference cannot exceed 100 characters.");
        }

        if (payment.PaymentReceivedAt == default || payment.PaymentReceivedAt.Date > DateTime.UtcNow.Date)
        {
            return PaymentVerificationResultDto.Failed("Enter a valid payment date that is not in the future.");
        }

        if (string.IsNullOrWhiteSpace(verifiedByUsername))
        {
            return PaymentVerificationResultDto.Failed("The verifying user is required.");
        }

        var paymentMethod = string.IsNullOrWhiteSpace(payment.PaymentMethod)
            ? "Bank deposit"
            : payment.PaymentMethod.Trim();

        if (paymentMethod.Length > 50)
        {
            return PaymentVerificationResultDto.Failed("The payment method cannot exceed 50 characters.");
        }

        await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

        try
        {
            var invoice = await _context.Invoices
                .Include(i => i.Items).ThenInclude(it => it.Enrollment)
                .FirstOrDefaultAsync(i => i.Id == invoiceId);

            if (invoice == null)
            {
                return PaymentVerificationResultDto.Failed("Challan invoice was not found.");
            }

            if (invoice.Status != InvoiceStatus.Pending)
            {
                return PaymentVerificationResultDto.Failed(
                    invoice.Status == InvoiceStatus.Verified
                        ? "This challan has already been verified."
                        : "Only pending challans can be verified.");
            }

            if (await _context.Invoices.AnyAsync(i =>
                    i.Id != invoice.Id && i.PaymentReference == paymentReference))
            {
                return PaymentVerificationResultDto.Failed("This payment reference has already been used for another challan.");
            }

            var includedItems = invoice.Items.Where(item => item.IsIncluded).ToList();
            if (includedItems.Count == 0)
            {
                return PaymentVerificationResultDto.Failed("This challan has no included candidates to release.");
            }

            if (includedItems.Any(item => item.Enrollment == null
                                          || item.Enrollment.SchoolId != invoice.SchoolId
                                          || item.Enrollment.AcademicYearId != invoice.AcademicYearId))
            {
                return PaymentVerificationResultDto.Failed("This challan contains candidates outside its school or academic year.");
            }

            var itemTotal = includedItems.Sum(item => item.FeeAmount);
            if (invoice.StudentCount != includedItems.Count || invoice.Amount != itemTotal || invoice.Amount <= 0)
            {
                return PaymentVerificationResultDto.Failed("The challan totals do not match its candidate items. Review or reject it before payment verification.");
            }

            var releasedCandidateCount = 0;
            if (string.Equals(invoice.InvoiceType, "enrollment", StringComparison.OrdinalIgnoreCase))
            {
                if (includedItems.Any(item => !string.IsNullOrWhiteSpace(item.Enrollment!.EnrollmentNumber)))
                {
                    return PaymentVerificationResultDto.Failed("At least one candidate already has an enrollment number. Review this challan before verifying it.");
                }

                releasedCandidateCount = await _enrollmentNumberService.AllotNumbersForInvoiceAsync(invoice.Id, verifiedByUsername);
                if (releasedCandidateCount != includedItems.Count)
                {
                    await transaction.RollbackAsync();
                    _context.ChangeTracker.Clear();
                    return PaymentVerificationResultDto.Failed("Unable to allot enrollment numbers for every candidate. No payment changes were saved.");
                }
            }
            else if (string.Equals(invoice.InvoiceType, "exam", StringComparison.OrdinalIgnoreCase))
            {
                var enrollmentIds = includedItems.Select(item => item.EnrollmentId).Distinct().ToList();
                var examForms = await _context.ExaminationForms
                    .Where(form => enrollmentIds.Contains(form.EnrollmentId)
                                   && form.SchoolId == invoice.SchoolId
                                   && form.AcademicYearId == invoice.AcademicYearId)
                    .ToListAsync();

                if (examForms.Count != enrollmentIds.Count || examForms.Any(form => form.Status != ExamFormStatus.Final))
                {
                    return PaymentVerificationResultDto.Failed("Every included candidate must have a final examination form before this payment can be verified.");
                }

                foreach (var form in examForms)
                {
                    form.Status = ExamFormStatus.Verified;
                    form.UpdatedAt = DateTime.UtcNow;
                }

                releasedCandidateCount = examForms.Count;
            }
            else
            {
                return PaymentVerificationResultDto.Failed("This challan type is not supported for payment verification.");
            }

            invoice.Status = InvoiceStatus.Verified;
            invoice.VerifiedAt = DateTime.UtcNow;
            invoice.PaymentReference = paymentReference;
            invoice.PaymentMethod = paymentMethod;
            invoice.PaymentReceivedAt = payment.PaymentReceivedAt.Date;
            invoice.PaymentVerifiedByUsername = verifiedByUsername.Trim();

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            await _activityLog.LogAsync("invoice_verified",
                $"Verified Invoice #{invoice.InvoiceNumber}; payment reference {MaskPaymentReference(paymentReference)}; released {releasedCandidateCount} candidates.",
                "Invoice", invoice.Id, null, verifiedByUsername);

            return new PaymentVerificationResultDto
            {
                Success = true,
                InvoiceId = invoice.Id,
                InvoiceNumber = invoice.InvoiceNumber,
                ReleasedCandidateCount = releasedCandidateCount
            };
        }
        catch (DbUpdateException)
        {
            _context.ChangeTracker.Clear();
            return PaymentVerificationResultDto.Failed("The payment could not be recorded. The reference may already be in use; refresh the invoice list and try again.");
        }
        catch (Exception)
        {
            _context.ChangeTracker.Clear();
            return PaymentVerificationResultDto.Failed("The payment could not be verified. No changes were saved.");
        }
    }

    public async Task<bool> RejectChallanAsync(int invoiceId, string reason, string rejectedByUsername)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Items).ThenInclude(it => it.Enrollment)
            .FirstOrDefaultAsync(i => i.Id == invoiceId);

        if (invoice == null || invoice.Status != InvoiceStatus.Pending) return false;

        invoice.Status = InvoiceStatus.Rejected;
        invoice.RejectionReason = reason;

        foreach (var item in invoice.Items)
        {
            item.IsIncluded = false;
            if (item.Enrollment != null && invoice.InvoiceType == "enrollment")
            {
                if (item.Enrollment.InvoiceId == invoice.Id)
                {
                    item.Enrollment.InvoiceId = null;
                    item.Enrollment.ChallanStatus = null;
                }
            }
        }

        await _context.SaveChangesAsync();

        await _activityLog.LogAsync("invoice_rejected",
            $"Rejected Invoice #{invoice.InvoiceNumber}. Reason: {reason}.",
            "Invoice", invoice.Id, null, rejectedByUsername);

        return true;
    }

    public async Task<bool> CancelChallanAsync(int invoiceId, string cancelledByUsername)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Items).ThenInclude(it => it.Enrollment)
            .FirstOrDefaultAsync(i => i.Id == invoiceId);

        if (invoice == null || invoice.Status != InvoiceStatus.Pending) return false;

        invoice.Status = InvoiceStatus.Cancelled;
        invoice.CancelledAt = DateTime.UtcNow;

        foreach (var item in invoice.Items)
        {
            item.IsIncluded = false;
            if (item.Enrollment != null && invoice.InvoiceType == "enrollment")
            {
                if (item.Enrollment.InvoiceId == invoice.Id)
                {
                    item.Enrollment.InvoiceId = null;
                    item.Enrollment.ChallanStatus = null;
                }
            }
        }

        await _context.SaveChangesAsync();

        await _activityLog.LogAsync("invoice_cancelled",
            $"Cancelled Invoice #{invoice.InvoiceNumber}.",
            "Invoice", invoice.Id, null, cancelledByUsername);

        return true;
    }

    private IQueryable<Enrollment> BuildEligibleQuery(
        int schoolId,
        int academicYearId,
        string challanType,
        string? classLevel = null,
        string? group = null,
        string? studentType = null)
    {
        var query = _context.Enrollments
            .Where(e => e.SchoolId == schoolId);

        if (academicYearId > 0)
        {
            query = query.Where(e => e.AcademicYearId == academicYearId || e.AcademicYearId == 0);
        }

        if (!string.IsNullOrEmpty(classLevel))
        {
            var variants = GetClassLevelVariants(classLevel);
            query = query.Where(e => variants.Contains(e.ClassLevel) || e.ClassLevel.StartsWith(classLevel));
        }

        if (!string.IsNullOrEmpty(group))
        {
            var grpClean = group.Trim().ToLowerInvariant().Replace(" ", "").Replace("-", "").Replace("_", "");
            query = query.Where(e => e.Group.ToLower().Replace(" ", "").Replace("-", "").Replace("_", "") == grpClean || e.Group.ToLower() == group.ToLower());
        }

        if (!string.IsNullOrEmpty(studentType))
        {
            var stLower = studentType.Trim().ToLowerInvariant();
            if (stLower == "fresh" || stLower == "regular")
            {
                query = query.Where(e => e.StudentType.ToLower() == "fresh" || e.StudentType.ToLower() == "regular");
            }
            else
            {
                query = query.Where(e => e.StudentType.ToLower() == stLower);
            }
        }

        // Exclude students who are actively included in any pending or verified invoice of the same type
        query = query.Where(e => !_context.InvoiceItems.Any(ii =>
            ii.EnrollmentId == e.Id &&
            ii.IsIncluded &&
            ii.InvoiceTypeSnapshot == challanType &&
            ii.Invoice != null &&
            ii.Invoice.InvoiceType == challanType &&
            ii.Invoice.SchoolId == schoolId &&
            ii.Invoice.AcademicYearId == academicYearId &&
            ii.Invoice.Status != InvoiceStatus.Rejected &&
            ii.Invoice.Status != InvoiceStatus.Cancelled));

        if (challanType == "enrollment")
        {
            query = query.Where(e => e.EnrollmentNumberAllottedAt == null && string.IsNullOrEmpty(e.EnrollmentNumber));
        }
        else if (challanType == "exam")
        {
            query = query.Where(e => !string.IsNullOrEmpty(e.EnrollmentNumber) &&
                _context.ExaminationForms.Any(ef => ef.EnrollmentId == e.Id && ef.SchoolId == schoolId && (ef.AcademicYearId == academicYearId || academicYearId == 0)));
        }

        return query;
    }

    private List<string> GetClassLevelVariants(string classLevel)
    {
        return classLevel switch
        {
            "SSC-I" or "9" or "SSC-I 1ST ANNUAL" or "SSC-I 2ND ANNUAL" or "SSC-I 1st Annual" or "SSC-I 2nd Annual"
                => new List<string> { "9", "SSC-I", "SSC-I 1ST ANNUAL", "SSC-I 2ND ANNUAL", "SSC-I 1st Annual", "SSC-I 2nd Annual" },
            "SSC-II" or "10" or "SSC-II 1ST ANNUAL" or "SSC-II 2ND ANNUAL" or "SSC-II 1st Annual" or "SSC-II 2nd Annual"
                => new List<string> { "10", "10-2nd", "SSC-II", "SSC-II 1ST ANNUAL", "SSC-II 2ND ANNUAL", "SSC-II 1st Annual", "SSC-II 2nd Annual" },
            "HSC-I" or "11" or "HSC-I 1ST ANNUAL" or "HSC-I 2ND ANNUAL" or "HSC-I 1st Annual" or "HSC-I 2nd Annual"
                => new List<string> { "11", "HSC-I", "HSC-I 1ST ANNUAL", "HSC-I 2ND ANNUAL", "HSC-I 1st Annual", "HSC-I 2nd Annual" },
            "HSC-II" or "12" or "HSC-II 1ST ANNUAL" or "HSC-II 2ND ANNUAL" or "HSC-II 1st Annual" or "HSC-II 2nd Annual"
                => new List<string> { "12", "12-2nd", "HSC-II", "HSC-II 1ST ANNUAL", "HSC-II 2ND ANNUAL", "HSC-II 1st Annual", "HSC-II 2nd Annual" },
            _ => new List<string> { classLevel }
        };
    }

    private static string MaskPaymentReference(string paymentReference)
    {
        return paymentReference.Length <= 4
            ? "****"
            : $"{paymentReference[..4]}…";
    }
}
