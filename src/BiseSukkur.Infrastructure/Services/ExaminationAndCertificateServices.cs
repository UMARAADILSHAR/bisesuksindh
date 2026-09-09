using BiseSukkur.Core.DTOs;
using BiseSukkur.Core.Entities;
using BiseSukkur.Core.Enums;
using BiseSukkur.Core.Interfaces;
using BiseSukkur.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using System.Text.Json;

namespace BiseSukkur.Infrastructure.Services;

public class ExaminationService : IExaminationService
{
    private readonly ApplicationDbContext _context;
    private readonly IActivityLogService _activityLog;

    public ExaminationService(ApplicationDbContext context, IActivityLogService activityLog)
    {
        _context = context;
        _activityLog = activityLog;
    }

    public async Task<List<ExaminationForm>> GetExaminationFormsBySchoolAsync(int schoolId, int? academicYearId = null, string? search = null)
    {
        var query = _context.ExaminationForms
            .Include(ef => ef.Enrollment)
            .Include(ef => ef.ExamCenter)
            .Include(ef => ef.Result)
            .Where(ef => ef.SchoolId == schoolId);

        if (academicYearId.HasValue)
        {
            query = query.Where(ef => ef.AcademicYearId == academicYearId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(ef =>
                (ef.Enrollment != null && ef.Enrollment.StudentName.ToLower().Contains(term)) ||
                (ef.Enrollment != null && ef.Enrollment.Cnic.Contains(term)) ||
                (ef.Enrollment != null && ef.Enrollment.EnrollmentNumber != null && ef.Enrollment.EnrollmentNumber.ToLower().Contains(term)) ||
                (ef.RollNumber != null && ef.RollNumber.Contains(term)));
        }

        return await query.OrderBy(ef => ef.Id).ToListAsync();
    }

    public async Task<PagedResult<ExaminationForm>> GetPagedExaminationFormsAsync(int schoolId, PagedRequest request)
    {
        var query = _context.ExaminationForms
            .AsNoTracking()
            .Include(ef => ef.Enrollment)
            .Include(ef => ef.ExamCenter)
            .Include(ef => ef.Result)
            .Where(ef => ef.SchoolId == schoolId);

        if (request.Filters.TryGetValue("academicYearId", out var yearStr) && int.TryParse(yearStr, out var yearId))
        {
            query = query.Where(ef => ef.AcademicYearId == yearId);
        }

        if (request.Filters.TryGetValue("classLevel", out var classLevel) && !string.IsNullOrWhiteSpace(classLevel))
        {
            query = query.Where(ef => ef.ClassLevel == classLevel);
        }

        if (request.Filters.TryGetValue("status", out var status) && !string.IsNullOrWhiteSpace(status)
            && Enum.TryParse<ExamFormStatus>(status, true, out var statusValue))
        {
            query = query.Where(ef => ef.Status == statusValue);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim().ToLower();
            query = query.Where(ef =>
                (ef.Enrollment != null && ef.Enrollment.StudentName.ToLower().Contains(term)) ||
                (ef.Enrollment != null && ef.Enrollment.Cnic.Contains(term)) ||
                (ef.Enrollment != null && ef.Enrollment.EnrollmentNumber != null && ef.Enrollment.EnrollmentNumber.ToLower().Contains(term)) ||
                (ef.RollNumber != null && ef.RollNumber.Contains(term)));
        }

        var totalCount = await query.CountAsync();

        query = request.SortBy?.ToLower() switch
        {
            "classlevel" => request.SortDescending ? query.OrderByDescending(ef => ef.ClassLevel) : query.OrderBy(ef => ef.ClassLevel),
            "rollnumber" => request.SortDescending ? query.OrderByDescending(ef => ef.RollNumber) : query.OrderBy(ef => ef.RollNumber),
            _ => request.SortDescending ? query.OrderByDescending(ef => ef.Id) : query.OrderBy(ef => ef.Id)
        };

        var items = await query.Skip(request.Skip).Take(request.Take).ToListAsync();
        return new PagedResult<ExaminationForm>(items, totalCount, request.PageNumber, request.Take);
    }

    public async Task<ExaminationMetricsDto> GetExaminationMetricsAsync(int schoolId, int? academicYearId = null)
    {
        var query = _context.ExaminationForms.Where(ef => ef.SchoolId == schoolId);
        if (academicYearId.HasValue)
        {
            query = query.Where(ef => ef.AcademicYearId == academicYearId.Value);
        }

        var total = await query.CountAsync();
        var part1 = await query.CountAsync(ef => ef.ClassLevel.Contains("-I") && !ef.ClassLevel.Contains("-II"));
        var part2 = await query.CountAsync(ef => ef.ClassLevel.Contains("-II"));
        var slipsIssued = await query.CountAsync(ef => !string.IsNullOrEmpty(ef.RollNumber));
        var verified = await query.CountAsync(ef => ef.Status == ExamFormStatus.Verified);

        return new ExaminationMetricsDto
        {
            TotalForms = total,
            Part1Count = part1,
            Part2Count = part2,
            SlipsIssued = slipsIssued,
            PendingSlips = total - slipsIssued,
            VerifiedCount = verified
        };
    }

    public async Task<ExaminationForm?> GetExaminationFormByIdAsync(int id)
    {
        return await _context.ExaminationForms
            .Include(ef => ef.Enrollment).ThenInclude(e => e!.School)
            .Include(ef => ef.ExamCenter)
            .Include(ef => ef.Result)
            .FirstOrDefaultAsync(ef => ef.Id == id);
    }

    public async Task<ExaminationForm?> GetExaminationFormByEnrollmentIdAsync(int enrollmentId)
    {
        return await _context.ExaminationForms
            .Include(ef => ef.Enrollment)
            .Include(ef => ef.ExamCenter)
            .Include(ef => ef.Result)
            .FirstOrDefaultAsync(ef => ef.EnrollmentId == enrollmentId);
    }

    public async Task<ExaminationForm> SaveExaminationFormAsync(ExaminationForm form, bool finalize = false)
    {
        if (form.SchoolId == 0 && form.EnrollmentId > 0)
        {
            var enroll = await _context.Enrollments.FindAsync(form.EnrollmentId);
            if (enroll != null)
            {
                form.SchoolId = enroll.SchoolId;
                if (form.AcademicYearId == 0) form.AcademicYearId = enroll.AcademicYearId;
            }
        }

        if (finalize || form.Status == ExamFormStatus.Draft)
        {
            form.Status = ExamFormStatus.Final;
        }

        if (form.Id == 0)
        {
            // Auto-assign school's exam center if available
            var assignedCenter = await _context.ExamCenterSchools
                .Where(ecs => ecs.SchoolId == form.SchoolId)
                .Select(ecs => ecs.ExamCenterId)
                .FirstOrDefaultAsync();

            if (assignedCenter > 0 && !form.ExamCenterId.HasValue)
            {
                form.ExamCenterId = assignedCenter;
            }

            _context.ExaminationForms.Add(form);
        }
        else
        {
            var existing = await _context.ExaminationForms.FindAsync(form.Id);
            if (existing != null)
            {
                existing.ClassLevel = form.ClassLevel;
                existing.Group = form.Group;
                existing.StudentType = form.StudentType;
                existing.PreviousSeatNumber = form.PreviousSeatNumber;
                existing.PreviousYear = form.PreviousYear;
                existing.SubjectsJson = form.SubjectsJson;
                if (form.ExamCenterId.HasValue) existing.ExamCenterId = form.ExamCenterId;
                if (finalize || form.Status == ExamFormStatus.Final) existing.Status = ExamFormStatus.Final;
                existing.UpdatedAt = DateTime.UtcNow;
                form = existing;
            }
        }

        await _context.SaveChangesAsync();
        return form;
    }

    public async Task<List<GapReportRowDto>> GetGapReportAsync(int schoolId, int academicYearId)
    {
        var verifiedEnrollments = await _context.Enrollments
            .Include(e => e.ExaminationForm)
            .Where(e => e.SchoolId == schoolId && e.AcademicYearId == academicYearId && !string.IsNullOrEmpty(e.EnrollmentNumber))
            .OrderBy(e => e.StudentName)
            .ToListAsync();

        return verifiedEnrollments.Select(e => new GapReportRowDto
        {
            EnrollmentId = e.Id,
            StudentName = e.StudentName,
            FatherName = e.FatherName ?? "",
            EnrollmentNumber = e.EnrollmentNumber ?? "",
            ClassLevel = e.ClassLevel,
            Group = e.Group,
            StudentType = e.StudentType,
            HasExamForm = e.ExaminationForm != null,
            RollNumber = e.ExaminationForm?.RollNumber,
            ExamStatus = e.ExaminationForm != null ? e.ExaminationForm.Status.ToString() : "Not Submitted"
        }).ToList();
    }

    public async Task<List<ExamCenter>> GetExamCentersAsync(int? districtId = null)
    {
        var query = _context.ExamCenters
            .Include(ec => ec.District)
            .Include(ec => ec.AssignedSchools).ThenInclude(asg => asg.School)
            .AsQueryable();

        if (districtId.HasValue)
        {
            query = query.Where(ec => ec.DistrictId == districtId.Value);
        }

        return await query.OrderBy(ec => ec.Name).ToListAsync();
    }

    public async Task<ExamCenter> SaveExamCenterAsync(ExamCenter center)
    {
        if (center.Id == 0)
        {
            _context.ExamCenters.Add(center);
        }
        else
        {
            var existing = await _context.ExamCenters.FindAsync(center.Id);
            if (existing != null)
            {
                existing.CenterCode = center.CenterCode;
                existing.Name = center.Name;
                existing.DistrictId = center.DistrictId;
                existing.SuperintendentName = center.SuperintendentName;
                existing.SuperintendentPhone = center.SuperintendentPhone;
                existing.Capacity = center.Capacity;
                existing.IsActive = center.IsActive;
                center = existing;
            }
        }
        await _context.SaveChangesAsync();
        return center;
    }

    public async Task<bool> AssignSchoolToCenterAsync(int examCenterId, int schoolId)
    {
        var exists = await _context.ExamCenterSchools
            .AnyAsync(ecs => ecs.ExamCenterId == examCenterId && ecs.SchoolId == schoolId);

        if (!exists)
        {
            _context.ExamCenterSchools.Add(new ExamCenterSchool
            {
                ExamCenterId = examCenterId,
                SchoolId = schoolId
            });
            await _context.SaveChangesAsync();
        }
        return true;
    }

    public async Task<List<ExamSchedule>> GetExamSchedulesAsync(int academicYearId, string? classLevel = null)
    {
        var query = _context.ExamSchedules.Where(es => es.AcademicYearId == academicYearId);
        if (!string.IsNullOrEmpty(classLevel))
        {
            query = query.Where(es => es.ClassLevel == classLevel);
        }
        return await query.OrderBy(es => es.ExamDate).ThenBy(es => es.Session).ToListAsync();
    }

    public async Task<ExamSchedule> SaveExamScheduleAsync(ExamSchedule schedule)
    {
        if (schedule.Id == 0)
        {
            _context.ExamSchedules.Add(schedule);
        }
        else
        {
            var existing = await _context.ExamSchedules.FindAsync(schedule.Id);
            if (existing != null)
            {
                existing.ClassLevel = schedule.ClassLevel;
                existing.Group = schedule.Group;
                existing.SubjectName = schedule.SubjectName;
                existing.ExamDate = schedule.ExamDate;
                existing.Session = schedule.Session;
                existing.PaperDurationMinutes = schedule.PaperDurationMinutes;
                existing.MaxMarks = schedule.MaxMarks;
                schedule = existing;
            }
        }
        await _context.SaveChangesAsync();
        return schedule;
    }

    public async Task<bool> DeleteExamScheduleAsync(int scheduleId)
    {
        var item = await _context.ExamSchedules.FindAsync(scheduleId);
        if (item == null) return false;
        _context.ExamSchedules.Remove(item);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> RunRollNumberAllotmentAsync(int academicYearId, string classLevel)
    {
        var forms = await _context.ExaminationForms
            .Include(ef => ef.Enrollment)
            .Where(ef =>
                ef.AcademicYearId == academicYearId &&
                ef.ClassLevel == classLevel &&
                ef.Status == ExamFormStatus.Verified &&
                string.IsNullOrEmpty(ef.RollNumber))
            .OrderBy(ef => ef.SchoolId)
            .ThenBy(ef => ef.Group)
            .ThenBy(ef => ef.Enrollment != null ? ef.Enrollment.StudentName : "")
            .ToListAsync();

        if (!forms.Any()) return 0;

        var seq = await _context.InvoiceSequences.FirstOrDefaultAsync(s => s.SequenceType == "exam");
        if (seq == null)
        {
            seq = new InvoiceSequence { SequenceType = "exam", LastNumber = 100000 };
            _context.InvoiceSequences.Add(seq);
            await _context.SaveChangesAsync();
        }

        int count = 0;
        foreach (var form in forms)
        {
            seq.LastNumber += 1;
            form.RollNumber = seq.LastNumber.ToString();
            form.UpdatedAt = DateTime.UtcNow;
            count++;
        }

        seq.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        await _activityLog.LogAsync("roll_allotment", $"Allotted roll numbers to {count} candidates for {classLevel}.");
        return count;
    }

    public async Task<Result> SaveResultMarksAsync(int examinationFormId, Dictionary<string, int> marks, string declaredBy)
    {
        var examForm = await _context.ExaminationForms.FindAsync(examinationFormId);
        if (examForm == null) throw new InvalidOperationException("Examination form not found.");

        int totalObtained = marks.Values.Sum();
        int totalMax = marks.Count * 100;
        decimal percentage = totalMax > 0 ? Math.Round((decimal)totalObtained / totalMax * 100, 2) : 0;

        string grade = percentage switch
        {
            >= 80 => "A-1",
            >= 70 => "A",
            >= 60 => "B",
            >= 50 => "C",
            >= 40 => "D",
            >= 33 => "E",
            _ => "Fail"
        };
        bool isPassed = percentage >= 33;

        var existingResult = await _context.Results.FirstOrDefaultAsync(r => r.ExaminationFormId == examinationFormId);
        if (existingResult == null)
        {
            existingResult = new Result
            {
                ExaminationFormId = examinationFormId,
                MarksJson = JsonSerializer.Serialize(marks),
                TotalObtained = totalObtained,
                TotalMaxMarks = totalMax,
                Percentage = percentage,
                Grade = grade,
                IsPassed = isPassed,
                DeclaredAt = DateTime.UtcNow
            };
            _context.Results.Add(existingResult);
        }
        else
        {
            existingResult.MarksJson = JsonSerializer.Serialize(marks);
            existingResult.TotalObtained = totalObtained;
            existingResult.TotalMaxMarks = totalMax;
            existingResult.Percentage = percentage;
            existingResult.Grade = grade;
            existingResult.IsPassed = isPassed;
            existingResult.DeclaredAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return existingResult;
    }

    public List<string> GetSubjectCatalog(string classLevel, string group)
    {
        var cls = (classLevel ?? "").Trim().ToUpper();
        var grp = (group ?? "").Trim().ToLower().Replace("-", "_").Replace(" ", "_");
        bool isGeneral = grp.Contains("general") || grp.Contains("arts") || grp.Contains("humanities");

        if (cls is "9" or "SSC-I")
        {
            return isGeneral
                ? new List<string> { "SINDHI", "ENGLISH-I", "ISLAMIAT", "GENERAL SCIENCE-I", "GENERAL MATHEMATICS-I", "CIVICS-I", "ISLAMIC HISTORY-I" }
                : new List<string> { "SINDHI", "ENGLISH-I", "ISLAMIAT", "CHEMISTRY-I", "PHYSICS-I", "BIOLOGY-I", "MATHEMATICS-I" };
        }

        if (cls is "10" or "SSC-II" or "10-2ND")
        {
            return isGeneral
                ? new List<string> { "ENGLISH-II", "PAKISTAN STUDIES", "GENERAL SCIENCE-II", "GENERAL MATHEMATICS-II", "CIVICS-II", "ISLAMIC HISTORY-II", "URDU SALEES" }
                : new List<string> { "ENGLISH-II", "PAKISTAN STUDIES", "PHYSICS-II", "CHEMISTRY-II", "MATHEMATICS-II", "BIOLOGY-II", "URDU SALEES" };
        }

        if (cls is "11" or "HSC-I")
        {
            if (grp.Contains("medical"))
                return new List<string> { "ENGLISH-I", "URDU-I", "ISLAMIC EDUCATION", "PHYSICS-I", "CHEMISTRY-I", "BIOLOGY-I" };
            if (grp.Contains("engineering"))
                return new List<string> { "ENGLISH-I", "URDU-I", "ISLAMIC EDUCATION", "PHYSICS-I", "CHEMISTRY-I", "MATHEMATICS-I" };
            if (grp.Contains("commerce"))
                return new List<string> { "ENGLISH-I", "URDU-I", "ISLAMIC EDUCATION", "PRINCIPLES OF ACCOUNTING-I", "PRINCIPLES OF ECONOMICS", "BUSINESS MATHEMATICS" };
            return new List<string> { "ENGLISH-I", "URDU-I", "ISLAMIC EDUCATION", "CIVICS-I", "ISLAMIC HISTORY-I", "EDUCATION-I" };
        }

        if (cls is "12" or "HSC-II" or "12-2ND")
        {
            if (grp.Contains("medical"))
                return new List<string> { "ENGLISH-II", "URDU-II", "PAKISTAN STUDIES", "PHYSICS-II", "CHEMISTRY-II", "BIOLOGY-II" };
            if (grp.Contains("engineering"))
                return new List<string> { "ENGLISH-II", "URDU-II", "PAKISTAN STUDIES", "PHYSICS-II", "CHEMISTRY-II", "MATHEMATICS-II" };
            if (grp.Contains("commerce"))
                return new List<string> { "ENGLISH-II", "URDU-II", "PAKISTAN STUDIES", "PRINCIPLES OF ACCOUNTING-II", "COMMERCIAL GEOGRAPHY", "BANKING" };
            return new List<string> { "ENGLISH-II", "URDU-II", "PAKISTAN STUDIES", "CIVICS-II", "ISLAMIC HISTORY-II", "EDUCATION-II" };
        }

        return new List<string> { "ENGLISH", "URDU", "ISLAMIAT", "GENERAL PAPER-I", "GENERAL PAPER-II" };
    }
}

public class CertificateService : ICertificateService
{
    private readonly ApplicationDbContext _context;
    private readonly IIpRateLimitService _ipRateLimit;

    public CertificateService(ApplicationDbContext context, IIpRateLimitService ipRateLimit)
    {
        _context = context;
        _ipRateLimit = ipRateLimit;
    }

    public async Task<Certificate?> GetCertificateByNumberAsync(string certNumber)
    {
        if (_ipRateLimit.IsRateLimited("certificate-verify", permitLimit: 20, window: TimeSpan.FromMinutes(1)))
        {
            return null;
        }

        return await _context.Certificates
            .Include(c => c.Result).ThenInclude(r => r!.ExaminationForm).ThenInclude(ef => ef!.Enrollment)
            .FirstOrDefaultAsync(c => c.CertificateNumber.ToLower() == certNumber.Trim().ToLower());
    }

    public async Task<Certificate?> GetCertificateByTokenAsync(string token)
    {
        if (_ipRateLimit.IsRateLimited("certificate-verify", permitLimit: 20, window: TimeSpan.FromMinutes(1)))
        {
            return null;
        }

        return await _context.Certificates
            .Include(c => c.Result).ThenInclude(r => r!.ExaminationForm).ThenInclude(ef => ef!.Enrollment)
            .FirstOrDefaultAsync(c => c.VerificationToken.ToLower() == token.Trim().ToLower());
    }

    public async Task<Certificate> GenerateCertificateAsync(int resultId)
    {
        var existing = await _context.Certificates.FirstOrDefaultAsync(c => c.ResultId == resultId);
        if (existing != null) return existing;

        var result = await _context.Results
            .Include(r => r.ExaminationForm).ThenInclude(ef => ef!.Enrollment).ThenInclude(e => e!.School)
            .FirstOrDefaultAsync(r => r.Id == resultId);

        if (result == null || result.ExaminationForm == null || result.ExaminationForm.Enrollment == null)
        {
            throw new InvalidOperationException("Result or student data not found.");
        }

        var student = result.ExaminationForm.Enrollment;
        var school = student.School;

        var token = $"VERIFY-HYD-{DateTime.UtcNow.Year}-{Guid.NewGuid().ToString()[..6].ToUpper()}";
        var count = await _context.Certificates.CountAsync() + 1;
        var certNo = $"BISE-HYD-{DateTime.UtcNow.Year}-{count:D5}";

        var cert = new Certificate
        {
            ResultId = resultId,
            CertificateNumber = certNo,
            VerificationToken = token,
            StudentName = student.StudentName,
            FatherName = student.FatherName ?? "",
            EnrollmentNumber = student.EnrollmentNumber ?? "",
            RollNumber = result.ExaminationForm.RollNumber ?? "",
            SchoolName = school?.Name ?? "",
            ClassLevel = result.ExaminationForm.ClassLevel,
            Group = result.ExaminationForm.Group,
            Grade = result.Grade,
            TotalMarksObtained = result.TotalObtained,
            TotalMaxMarks = result.TotalMaxMarks,
            IssueDate = DateTime.UtcNow
        };

        _context.Certificates.Add(cert);
        await _context.SaveChangesAsync();
        return cert;
    }

    public async Task<List<Certificate>> GetCertificatesAsync(int? academicYearId = null, string? search = null)
    {
        var query = _context.Certificates
            .Include(c => c.Result).ThenInclude(r => r!.ExaminationForm)
            .AsQueryable();

        if (academicYearId.HasValue)
        {
            query = query.Where(c => c.Result != null && c.Result.ExaminationForm != null && c.Result.ExaminationForm.AcademicYearId == academicYearId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(c =>
                c.CertificateNumber.ToLower().Contains(term) ||
                c.VerificationToken.ToLower().Contains(term) ||
                c.StudentName.ToLower().Contains(term) ||
                c.RollNumber.Contains(term) ||
                c.EnrollmentNumber.ToLower().Contains(term));
        }

        return await query.OrderByDescending(c => c.IssueDate).ToListAsync();
    }

    public byte[] GenerateQrCodePng(string payload)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrCodeData);
        return qrCode.GetGraphic(20);
    }
}
