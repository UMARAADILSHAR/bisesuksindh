using BiseSukkur.Core.DTOs;
using BiseSukkur.Core.Entities;
using BiseSukkur.Core.Enums;
using BiseSukkur.Core.Helpers;
using BiseSukkur.Core.Interfaces;
using BiseSukkur.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Text.RegularExpressions;

namespace BiseSukkur.Infrastructure.Services;

public class EnrollmentNumberService : IEnrollmentNumberService
{
    private readonly ApplicationDbContext _context;
    private readonly IActivityLogService _activityLog;

    public EnrollmentNumberService(ApplicationDbContext context, IActivityLogService activityLog)
    {
        _context = context;
        _activityLog = activityLog;
    }

    public string ResolveGroupCode(string group) => EnrollmentCodeHelper.ResolveGroupCode(group);

    public string ResolveDistrictCode(District? district) => EnrollmentCodeHelper.ResolveDistrictCode(district);

    public async Task<int> AllotNumbersForInvoiceAsync(int invoiceId, string username)
    {
        var invoice = await _context.Invoices
            .Include(i => i.School).ThenInclude(s => s!.District)
            .Include(i => i.AcademicYear)
            .Include(i => i.Items).ThenInclude(it => it.Enrollment)
            .FirstOrDefaultAsync(i => i.Id == invoiceId);

        if (invoice == null || invoice.Status == InvoiceStatus.Rejected || invoice.Status == InvoiceStatus.Cancelled)
        {
            return 0;
        }

        var school = invoice.School;
        if (school == null) return 0;

        var districtCode = ResolveDistrictCode(school.District);
        var zone = school.Zone > 0 ? school.Zone : 1;
        var schoolCode = !string.IsNullOrEmpty(school.Code)
            ? school.Code.PadLeft(3, '0')
            : school.Id.ToString().PadLeft(3, '0');

        var yearName = invoice.AcademicYear?.YearName ?? DateTime.UtcNow.Year.ToString();
        var digitsOnly = Regex.Replace(yearName, @"[^\d]", "");
        var yearSuffix = digitsOnly.Length >= 2 ? digitsOnly[^2..] : "26";

        int allottedCount = 0;

        foreach (var item in invoice.Items.Where(it => it.IsIncluded && it.Enrollment != null))
        {
            var enrollment = item.Enrollment!;
            if (string.IsNullOrEmpty(enrollment.EnrollmentNumber))
            {
                var groupCode = ResolveGroupCode(enrollment.Group);

                // Find or create sequence row
                var seq = await _context.InvoiceSequences.FirstOrDefaultAsync(s =>
                    s.SequenceType == "enrollment" &&
                    s.AcademicYearId == invoice.AcademicYearId &&
                    s.SchoolId == school.Id &&
                    s.DistrictCode == districtCode &&
                    s.Zone == zone &&
                    s.GroupCode == groupCode);

                if (seq == null)
                {
                    seq = new InvoiceSequence
                    {
                        SequenceType = "enrollment",
                        AcademicYearId = invoice.AcademicYearId,
                        SchoolId = school.Id,
                        DistrictCode = districtCode,
                        Zone = zone,
                        GroupCode = groupCode,
                        LastNumber = 0
                    };
                    _context.InvoiceSequences.Add(seq);
                    await _context.SaveChangesAsync();
                }

                seq.LastNumber += 1;
                seq.UpdatedAt = DateTime.UtcNow;

                var formattedSeq = seq.LastNumber.ToString("D4");
                var enrollmentNumber = EnrollmentCodeHelper.FormatEnrollmentNumber(
                    yearSuffix, groupCode, districtCode, zone, schoolCode, seq.LastNumber);

                enrollment.EnrollmentNumber = enrollmentNumber;
                enrollment.EnrollmentNumberAllottedAt = DateTime.UtcNow;
                enrollment.Status = EnrollmentStatus.Verified;
                enrollment.ChallanStatus = "paid";
                enrollment.InvoiceId = invoice.Id;
                enrollment.UpdatedAt = DateTime.UtcNow;

                allottedCount++;
            }
        }

        await _context.SaveChangesAsync();

        await _activityLog.LogAsync("enrollment_allotment",
            $"Allotted {allottedCount} registration sequences for Invoice #{invoice.InvoiceNumber}.",
            "Invoice", invoice.Id, null, username);

        return allottedCount;
    }

    public async Task<string> AllotSingleEnrollmentNumberAsync(int enrollmentId, string username)
    {
        var enrollment = await _context.Enrollments
            .Include(e => e.School).ThenInclude(s => s!.District)
            .Include(e => e.AcademicYear)
            .FirstOrDefaultAsync(e => e.Id == enrollmentId);

        if (enrollment == null) return string.Empty;
        if (!string.IsNullOrEmpty(enrollment.EnrollmentNumber)) return enrollment.EnrollmentNumber;

        var school = enrollment.School;
        var districtCode = ResolveDistrictCode(school?.District);
        var zone = school != null && school.Zone > 0 ? school.Zone : 1;
        var schoolCode = school != null && !string.IsNullOrEmpty(school.Code)
            ? school.Code.PadLeft(3, '0')
            : (school?.Id ?? 1).ToString().PadLeft(3, '0');

        var yearName = enrollment.AcademicYear?.YearName ?? DateTime.UtcNow.Year.ToString();
        var digitsOnly = Regex.Replace(yearName, @"[^\d]", "");
        var yearSuffix = digitsOnly.Length >= 2 ? digitsOnly[^2..] : "26";
        var groupCode = ResolveGroupCode(enrollment.Group);

        var seq = await _context.InvoiceSequences.FirstOrDefaultAsync(s =>
            s.SequenceType == "enrollment" &&
            s.AcademicYearId == enrollment.AcademicYearId &&
            s.SchoolId == (school != null ? school.Id : null) &&
            s.DistrictCode == districtCode &&
            s.Zone == zone &&
            s.GroupCode == groupCode);

        if (seq == null)
        {
            seq = new InvoiceSequence
            {
                SequenceType = "enrollment",
                AcademicYearId = enrollment.AcademicYearId,
                SchoolId = school?.Id,
                DistrictCode = districtCode,
                Zone = zone,
                GroupCode = groupCode,
                LastNumber = 0
            };
            _context.InvoiceSequences.Add(seq);
            await _context.SaveChangesAsync();
        }

        seq.LastNumber += 1;
        seq.UpdatedAt = DateTime.UtcNow;

        var formattedSeq = seq.LastNumber.ToString("D4");
        var enrollmentNumber = EnrollmentCodeHelper.FormatEnrollmentNumber(
            yearSuffix, groupCode, districtCode, zone, schoolCode, seq.LastNumber);

        enrollment.EnrollmentNumber = enrollmentNumber;
        enrollment.EnrollmentNumberAllottedAt = DateTime.UtcNow;
        enrollment.Status = EnrollmentStatus.Verified;
        enrollment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return enrollmentNumber;
    }
}

public class EnrollmentService : IEnrollmentService
{
    private readonly ApplicationDbContext _context;
    private readonly IActivityLogService _activityLog;
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;
    private readonly IPermissionService? _permissions;

    private const long MaxPhotoBytes = 2 * 1024 * 1024;

    public EnrollmentService(
        ApplicationDbContext context,
        IActivityLogService activityLog)
        : this(context, activityLog, null!, null!, null)
    {
    }

    public EnrollmentService(
        ApplicationDbContext context,
        IActivityLogService activityLog,
        IWebHostEnvironment environment,
        IConfiguration configuration,
        IPermissionService? permissions = null)
    {
        _context = context;
        _activityLog = activityLog;
        _environment = environment;
        _configuration = configuration;
        _permissions = permissions;
    }

    public async Task<List<Enrollment>> GetEnrollmentsBySchoolAsync(int schoolId, int? academicYearId = null, string? search = null, string? classLevel = null)
    {
        var query = _context.Enrollments
            .Include(e => e.School)
            .Include(e => e.AcademicYear)
            .Include(e => e.Invoice)
            .Where(e => e.SchoolId == schoolId);

        if (academicYearId.HasValue)
        {
            query = query.Where(e => e.AcademicYearId == academicYearId.Value);
        }

        if (!string.IsNullOrWhiteSpace(classLevel))
        {
            query = query.Where(e => e.ClassLevel == classLevel);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(e => e.StudentName.ToLower().Contains(term)
                || (e.FatherName != null && e.FatherName.ToLower().Contains(term))
                || (e.GrNumber != null && e.GrNumber.ToLower().Contains(term))
                || e.Cnic.Contains(term)
                || (e.EnrollmentNumber != null && e.EnrollmentNumber.ToLower().Contains(term)));
        }

        return await query.OrderBy(e => e.Id).ToListAsync();
    }

    public async Task<PagedResult<Enrollment>> GetPagedEnrollmentsAsync(int schoolId, PagedRequest request)
    {
        var query = _context.Enrollments
            .AsNoTracking()
            .Include(e => e.School)
            .Include(e => e.AcademicYear)
            .Include(e => e.Invoice)
            .Where(e => e.SchoolId == schoolId);

        if (request.Filters.TryGetValue("academicYearId", out var yearStr) && int.TryParse(yearStr, out var yearId))
        {
            query = query.Where(e => e.AcademicYearId == yearId);
        }

        if (request.Filters.TryGetValue("classLevel", out var classLevel) && !string.IsNullOrWhiteSpace(classLevel))
        {
            query = query.Where(e => e.ClassLevel == classLevel);
        }

        if (request.Filters.TryGetValue("group", out var group) && !string.IsNullOrWhiteSpace(group))
        {
            query = query.Where(e => e.Group == group);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim().ToLower();
            query = query.Where(e => e.StudentName.ToLower().Contains(term)
                || (e.FatherName != null && e.FatherName.ToLower().Contains(term))
                || (e.GrNumber != null && e.GrNumber.ToLower().Contains(term))
                || e.Cnic.Contains(term)
                || (e.EnrollmentNumber != null && e.EnrollmentNumber.ToLower().Contains(term)));
        }

        var totalCount = await query.CountAsync();

        query = request.SortBy?.ToLower() switch
        {
            "studentname" => request.SortDescending ? query.OrderByDescending(e => e.StudentName) : query.OrderBy(e => e.StudentName),
            "grnumber" => request.SortDescending ? query.OrderByDescending(e => e.GrNumber) : query.OrderBy(e => e.GrNumber),
            "classlevel" => request.SortDescending ? query.OrderByDescending(e => e.ClassLevel) : query.OrderBy(e => e.ClassLevel),
            "enrollmentnumber" => request.SortDescending ? query.OrderByDescending(e => e.EnrollmentNumber) : query.OrderBy(e => e.EnrollmentNumber),
            _ => request.SortDescending ? query.OrderByDescending(e => e.Id) : query.OrderBy(e => e.Id)
        };

        var items = await query.Skip(request.Skip).Take(request.Take).ToListAsync();
        return new PagedResult<Enrollment>(items, totalCount, request.PageNumber, request.Take);
    }

    public async Task<EnrollmentMetricsDto> GetEnrollmentMetricsAsync(int schoolId, int? academicYearId = null)
    {
        var query = _context.Enrollments.Where(e => e.SchoolId == schoolId);
        if (academicYearId.HasValue)
        {
            query = query.Where(e => e.AcademicYearId == academicYearId.Value);
        }

        var total = await query.CountAsync();
        var ssc = await query.CountAsync(e => e.ClassLevel.StartsWith("SSC"));
        var hsc = await query.CountAsync(e => e.ClassLevel.StartsWith("HSC"));
        var allotted = await query.CountAsync(e => !string.IsNullOrEmpty(e.EnrollmentNumber));
        var pendingChallan = await query.CountAsync(e => string.IsNullOrEmpty(e.EnrollmentNumber) && e.InvoiceItems.Any(ii => ii.IsIncluded));
        var unInvoiced = await query.CountAsync(e => string.IsNullOrEmpty(e.EnrollmentNumber) && !e.InvoiceItems.Any(ii => ii.IsIncluded));

        return new EnrollmentMetricsDto
        {
            TotalEnrolled = total,
            SscCount = ssc,
            HscCount = hsc,
            AllottedCount = allotted,
            ChallanedPendingCount = pendingChallan,
            UnInvoicedCount = unInvoiced
        };
    }

    public async Task<Enrollment?> GetEnrollmentByIdAsync(int id)
    {
        return await _context.Enrollments
            .Include(e => e.School).ThenInclude(s => s!.District)
            .Include(e => e.AcademicYear)
            .Include(e => e.Invoice)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<Enrollment?> GetEnrollmentByCnicAsync(string cnic, int academicYearId)
    {
        var cleanCnic = cnic.Trim().Replace("-", "");
        return await _context.Enrollments
            .Include(e => e.School)
            .FirstOrDefaultAsync(e => e.AcademicYearId == academicYearId && e.Cnic.Replace("-", "") == cleanCnic);
    }

    public async Task<bool> CheckCnicExistsAsync(string cnic, int academicYearId, int? excludeEnrollmentId = null)
    {
        var cleanCnic = cnic.Trim().Replace("-", "");
        var query = _context.Enrollments.Where(e => e.AcademicYearId == academicYearId && e.Cnic.Replace("-", "") == cleanCnic);
        if (excludeEnrollmentId.HasValue)
        {
            query = query.Where(e => e.Id != excludeEnrollmentId.Value);
        }
        return await query.AnyAsync();
    }

    public async Task<Enrollment> SaveEnrollmentAsync(Enrollment enrollment, bool finalize = false)
    {
        if (_permissions != null && !await _permissions.HasPermissionAsync(Permissions.EnrollmentManage, enrollment.SchoolId, null))
            throw new UnauthorizedAccessException("You do not have permission to manage enrollments for this school.");

        // Capitalize candidate personal names
        enrollment.StudentName = enrollment.StudentName.Trim().ToUpperInvariant();
        if (!string.IsNullOrWhiteSpace(enrollment.FatherName))
            enrollment.FatherName = enrollment.FatherName.Trim().ToUpperInvariant();
        if (!string.IsNullOrWhiteSpace(enrollment.Surname))
            enrollment.Surname = enrollment.Surname.Trim().ToUpperInvariant();
        if (!string.IsNullOrWhiteSpace(enrollment.IdentificationMark))
            enrollment.IdentificationMark = enrollment.IdentificationMark.Trim().ToUpperInvariant();

        if (finalize)
        {
            enrollment.Status = EnrollmentStatus.Final;
        }

        if (enrollment.Id == 0)
        {
            _context.Enrollments.Add(enrollment);
        }
        else
        {
            var existing = await _context.Enrollments.FindAsync(enrollment.Id);
            if (existing != null)
            {
                existing.StudentName = enrollment.StudentName;
                existing.FatherName = enrollment.FatherName;
                existing.FatherCnic = enrollment.FatherCnic;
                existing.Surname = enrollment.Surname;
                existing.GrNumber = enrollment.GrNumber;
                existing.IdentificationMark = enrollment.IdentificationMark;
                existing.Cnic = enrollment.Cnic;
                existing.DateOfBirth = enrollment.DateOfBirth;
                existing.DateOfBirthWords = enrollment.DateOfBirthWords;
                existing.Gender = enrollment.Gender;
                existing.Medium = enrollment.Medium;
                existing.Religion = enrollment.Religion;
                existing.Nationality = enrollment.Nationality;
                existing.Address = enrollment.Address;
                existing.MobileNumber = enrollment.MobileNumber;
                existing.PhotoPath = enrollment.PhotoPath;
                existing.ClassLevel = enrollment.ClassLevel;
                existing.Group = enrollment.Group;
                existing.StudentType = enrollment.StudentType;
                existing.SubjectsJson = enrollment.SubjectsJson;
                existing.AdmissionDate = enrollment.AdmissionDate;
                existing.BoardRegNo = enrollment.BoardRegNo;
                existing.EligibilityNo = enrollment.EligibilityNo;
                existing.PreviousBoard = enrollment.PreviousBoard;
                existing.BoardPassingDate = enrollment.BoardPassingDate;
                if (finalize || enrollment.Status == EnrollmentStatus.Final)
                {
                    existing.Status = EnrollmentStatus.Final;
                }
                existing.UpdatedAt = DateTime.UtcNow;
                enrollment = existing;
            }
        }

        await _context.SaveChangesAsync();
        await _activityLog.LogAsync("enrollment_save",
            $"Saved enrollment record for candidate '{enrollment.StudentName}' (Status: {enrollment.Status}).",
            "Enrollment", enrollment.Id);

        return enrollment;
    }

    public async Task<string> SaveEnrollmentPhotoAsync(int schoolId, int enrollmentId, byte[] fileBytes, string contentType, string originalFileName)
    {
        if (_permissions != null && !await _permissions.HasPermissionAsync(Permissions.EnrollmentManage, schoolId, null))
            throw new UnauthorizedAccessException("You do not have permission to manage enrollment photos for this school.");

        if (fileBytes.Length == 0 || fileBytes.Length > MaxPhotoBytes)
        {
            throw new InvalidOperationException("Photo must be between 1 byte and 2 MB.");
        }

        if (!HasValidImageSignature(fileBytes, contentType))
            throw new InvalidOperationException("The uploaded file is not a valid image.");

        var allowedTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg", "image/png", "image/webp"
        };
        if (!allowedTypes.Contains(contentType))
        {
            throw new InvalidOperationException("Photo must be JPEG, PNG, or WebP.");
        }

        var enrollment = await _context.Enrollments.FirstOrDefaultAsync(e => e.Id == enrollmentId && e.SchoolId == schoolId)
            ?? throw new InvalidOperationException("Enrollment record not found.");

        var relativeRoot = _configuration["FileStorage:PhotoUploadPath"] ?? "uploads/photos";
        var extension = contentType switch
        {
            "image/png" => ".png",
            "image/webp" => ".webp",
            _ => ".jpg"
        };

        var safeName = $"{enrollmentId}_{Guid.NewGuid():N}{extension}";
        var webRelativeDir = $"{relativeRoot.Trim('/')}/{schoolId}";
        var physicalDir = Path.Combine(_environment.WebRootPath, webRelativeDir.Replace('/', Path.DirectorySeparatorChar));

        Directory.CreateDirectory(physicalDir);

        var physicalPath = Path.Combine(physicalDir, safeName);
        await File.WriteAllBytesAsync(physicalPath, fileBytes);

        var webPath = $"/{webRelativeDir}/{safeName}".Replace('\\', '/');
        enrollment.PhotoPath = webPath;
        enrollment.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return webPath;
    }

    public async Task<bool> DeleteEnrollmentAsync(int id, int schoolId)
    {
        if (_permissions != null && !await _permissions.HasPermissionAsync(Permissions.EnrollmentManage, schoolId, null))
            return false;

        var enrollment = await _context.Enrollments.FirstOrDefaultAsync(e => e.Id == id && e.SchoolId == schoolId);
        if (enrollment == null) return false;

        // Cannot delete if paid or verified or tied to non-rejected invoice
        if (enrollment.Status == EnrollmentStatus.Verified || !string.IsNullOrEmpty(enrollment.EnrollmentNumber))
        {
            return false;
        }

        if (enrollment.InvoiceId.HasValue)
        {
            var invoice = await _context.Invoices.FindAsync(enrollment.InvoiceId.Value);
            if (invoice != null && invoice.Status != InvoiceStatus.Rejected && invoice.Status != InvoiceStatus.Cancelled)
            {
                return false;
            }
        }

        _context.Enrollments.Remove(enrollment);
        await _context.SaveChangesAsync();
        return true;
    }

    private static bool HasValidImageSignature(byte[] bytes, string contentType)
    {
        if (contentType.Equals("image/jpeg", StringComparison.OrdinalIgnoreCase))
            return bytes.Length >= 3 && bytes[0] == 0xff && bytes[1] == 0xd8 && bytes[2] == 0xff;
        if (contentType.Equals("image/png", StringComparison.OrdinalIgnoreCase))
            return bytes.Length >= 8 && bytes.AsSpan(0, 8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 });
        if (contentType.Equals("image/webp", StringComparison.OrdinalIgnoreCase))
            return bytes.Length >= 12 && bytes.AsSpan(0, 4).SequenceEqual("RIFF"u8) && bytes.AsSpan(8, 4).SequenceEqual("WEBP"u8);
        return false;
    }

    public async Task<Enrollment?> LookupPreviousSscRecordAsync(string rollNumber, string? year = null)
    {
        var cleanRoll = (rollNumber ?? "").Trim();
        if (string.IsNullOrEmpty(cleanRoll)) return null;

        // 1. First check ExaminationForm records for matching Seat / Roll Number
        var examForm = await _context.ExaminationForms
            .Include(ef => ef.Enrollment).ThenInclude(e => e!.School)
            .Include(ef => ef.AcademicYear)
            .FirstOrDefaultAsync(ef => ef.RollNumber == cleanRoll || (ef.Enrollment != null && (ef.Enrollment.BoardRegNo == cleanRoll || ef.Enrollment.EnrollmentNumber == cleanRoll)));

        if (examForm?.Enrollment != null)
        {
            return examForm.Enrollment;
        }

        // 2. Lookup in Enrollment database by Enrollment Number, Board Reg No, or CNIC
        return await _context.Enrollments
            .Include(e => e.School)
            .FirstOrDefaultAsync(e => e.EnrollmentNumber == cleanRoll || e.BoardRegNo == cleanRoll || e.Cnic == cleanRoll || e.GrNumber == cleanRoll);
    }
}
