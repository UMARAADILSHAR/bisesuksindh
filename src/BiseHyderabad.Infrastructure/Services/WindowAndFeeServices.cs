using BiseHyderabad.Core.DTOs;
using BiseHyderabad.Core.Helpers;
using BiseHyderabad.Core.Entities;
using BiseHyderabad.Core.Interfaces;
using BiseHyderabad.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BiseHyderabad.Infrastructure.Services;

public class WindowService : IWindowService
{
    private readonly ApplicationDbContext _context;

    public WindowService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AcademicYear?> GetActiveAcademicYearAsync()
    {
        return await _context.AcademicYears
            .Include(a => a.FeeRates)
            .FirstOrDefaultAsync(a => a.IsActive);
    }

    public async Task<List<AcademicYear>> GetAllAcademicYearsAsync()
    {
        return await _context.AcademicYears.OrderByDescending(a => a.YearName).ToListAsync();
    }

    public async Task<AcademicYear> SaveAcademicYearAsync(AcademicYear year)
    {
        if (year.Id == 0)
        {
            if (year.IsActive)
            {
                var others = await _context.AcademicYears.Where(a => a.IsActive).ToListAsync();
                foreach (var o in others) o.IsActive = false;
            }
            _context.AcademicYears.Add(year);
        }
        else
        {
            var existing = await _context.AcademicYears.FindAsync(year.Id);
            if (existing != null)
            {
                if (year.IsActive && !existing.IsActive)
                {
                    var others = await _context.AcademicYears.Where(a => a.Id != year.Id && a.IsActive).ToListAsync();
                    foreach (var o in others) o.IsActive = false;
                }

                existing.YearName = year.YearName;
                existing.StartDate = year.StartDate;
                existing.EndDate = year.EndDate;
                existing.IsActive = year.IsActive;

                existing.IsEnrollmentOpen = year.IsEnrollmentOpen;
                existing.EnrollmentOpenStart = year.EnrollmentOpenStart;
                existing.EnrollmentOpenEnd = year.EnrollmentOpenEnd;
                existing.EnrollmentGraceEnd = year.EnrollmentGraceEnd;
                existing.IsEnrollmentGraceEnabled = year.IsEnrollmentGraceEnabled;
                existing.EnrollmentLateFeeType = year.EnrollmentLateFeeType;
                existing.EnrollmentLateFeeAmount = year.EnrollmentLateFeeAmount;

                existing.IsExamOpen = year.IsExamOpen;
                existing.ExamOpenStart = year.ExamOpenStart;
                existing.ExamOpenEnd = year.ExamOpenEnd;
                existing.ExamGraceEnd = year.ExamGraceEnd;
                existing.IsExamGraceEnabled = year.IsExamGraceEnabled;
                existing.ExamLateFeeType = year.ExamLateFeeType;
                existing.ExamLateFeeAmount = year.ExamLateFeeAmount;

                year = existing;
            }
        }
        await _context.SaveChangesAsync();
        return year;
    }

    public async Task<TimelineDto> ResolveTimelineAsync(int schoolId, string type = "enrollment")
    {
        var school = await _context.Schools.FindAsync(schoolId);
        if (school == null) return await ResolveGlobalTimelineAsync(type);

        var schoolOverride = await _context.WindowOverrides
            .FirstOrDefaultAsync(o => o.ScopeType == "school" && o.ScopeId == schoolId && o.WindowType == type);

        if (schoolOverride != null)
        {
            return new TimelineDto
            {
                Source = "school",
                PortalOpen = true,
                NormalStart = schoolOverride.NormalStart,
                NormalEnd = schoolOverride.NormalEnd,
                GraceEnd = schoolOverride.GraceEnd,
                GraceEnabled = schoolOverride.IsGraceEnabled,
                LateFeeType = schoolOverride.LateFeeType,
                LateFeeAmount = schoolOverride.LateFeeAmount
            };
        }

        var districtOverride = await _context.WindowOverrides
            .FirstOrDefaultAsync(o => o.ScopeType == "district" && o.ScopeId == school.DistrictId && o.WindowType == type);

        if (districtOverride != null)
        {
            return new TimelineDto
            {
                Source = "district",
                PortalOpen = true,
                NormalStart = districtOverride.NormalStart,
                NormalEnd = districtOverride.NormalEnd,
                GraceEnd = districtOverride.GraceEnd,
                GraceEnabled = districtOverride.IsGraceEnabled,
                LateFeeType = districtOverride.LateFeeType,
                LateFeeAmount = districtOverride.LateFeeAmount
            };
        }

        return await ResolveGlobalTimelineAsync(type);
    }

    public async Task<TimelineDto> ResolveGlobalTimelineAsync(string type = "enrollment")
    {
        var activeYear = await GetActiveAcademicYearAsync();
        if (activeYear == null)
        {
            return new TimelineDto { Source = "global", PortalOpen = false };
        }

        if (type == "enrollment")
        {
            return new TimelineDto
            {
                Source = "global",
                PortalOpen = activeYear.IsEnrollmentOpen,
                NormalStart = activeYear.EnrollmentOpenStart,
                NormalEnd = activeYear.EnrollmentOpenEnd,
                GraceEnd = activeYear.EnrollmentGraceEnd,
                GraceEnabled = activeYear.IsEnrollmentGraceEnabled,
                LateFeeType = activeYear.EnrollmentLateFeeType,
                LateFeeAmount = activeYear.EnrollmentLateFeeAmount
            };
        }

        return new TimelineDto
        {
            Source = "global",
            PortalOpen = activeYear.IsExamOpen,
            NormalStart = activeYear.ExamOpenStart,
            NormalEnd = activeYear.ExamOpenEnd,
            GraceEnd = activeYear.ExamGraceEnd,
            GraceEnabled = activeYear.IsExamGraceEnabled,
            LateFeeType = activeYear.ExamLateFeeType,
            LateFeeAmount = activeYear.ExamLateFeeAmount
        };
    }

    public async Task<WindowStatusSummaryDto> GetWindowStatusSummaryAsync(string type = "enrollment", int? schoolId = null)
    {
        var now = DateTime.UtcNow;
        var timeline = schoolId.HasValue
            ? await ResolveTimelineAsync(schoolId.Value, type)
            : await ResolveGlobalTimelineAsync(type);

        string phase = WindowPhaseHelper.ResolvePhase(now, timeline);
        DateTime? endsAt = null;

        if (phase == "normal")
        {
            endsAt = timeline.NormalEnd;
        }
        else if (phase == "grace")
        {
            endsAt = timeline.GraceEnd;
        }

        string label = WindowPhaseHelper.ResolvePhaseLabel(phase);

        string? countdown = null;
        if (endsAt.HasValue && endsAt.Value > now)
        {
            var diff = endsAt.Value - now;
            countdown = diff.TotalDays >= 1
                ? $"{(int)diff.TotalDays}d {diff.Hours}h remaining"
                : $"{diff.Hours}h {diff.Minutes}m remaining";
        }

        return new WindowStatusSummaryDto
        {
            Phase = phase,
            Label = label,
            PortalOpen = timeline.PortalOpen,
            EndsAt = endsAt,
            Countdown = countdown
        };
    }

    public async Task<List<WindowOverride>> GetOverridesAsync(string? windowType = null)
    {
        var query = _context.WindowOverrides.AsQueryable();
        if (!string.IsNullOrEmpty(windowType))
        {
            query = query.Where(o => o.WindowType == windowType);
        }
        return await query.OrderByDescending(o => o.CreatedAt).ToListAsync();
    }

    public async Task<WindowOverride> SaveOverrideAsync(WindowOverride windowOverride)
    {
        if (windowOverride.Id == 0)
        {
            _context.WindowOverrides.Add(windowOverride);
        }
        else
        {
            var existing = await _context.WindowOverrides.FindAsync(windowOverride.Id);
            if (existing != null)
            {
                existing.NormalStart = windowOverride.NormalStart;
                existing.NormalEnd = windowOverride.NormalEnd;
                existing.GraceEnd = windowOverride.GraceEnd;
                existing.IsGraceEnabled = windowOverride.IsGraceEnabled;
                existing.LateFeeType = windowOverride.LateFeeType;
                existing.LateFeeAmount = windowOverride.LateFeeAmount;
                existing.Reason = windowOverride.Reason;
                windowOverride = existing;
            }
        }
        await _context.SaveChangesAsync();
        return windowOverride;
    }

    public async Task<bool> DeleteOverrideAsync(int id)
    {
        var item = await _context.WindowOverrides.FindAsync(id);
        if (item == null) return false;
        _context.WindowOverrides.Remove(item);
        await _context.SaveChangesAsync();
        return true;
    }
}

public class FeeRateService : IFeeRateService
{
    private readonly ApplicationDbContext _context;

    public FeeRateService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<FeeRate>> GetFeeRatesAsync(int academicYearId, string? feeType = null)
    {
        var query = _context.FeeRates.Where(f => f.AcademicYearId == academicYearId);
        if (!string.IsNullOrEmpty(feeType))
        {
            query = query.Where(f => f.FeeType == feeType);
        }
        return await query.OrderBy(f => f.FeeType).ThenBy(f => f.ClassLevel).ThenBy(f => f.GroupName).ToListAsync();
    }

    public async Task<FeeRate> SaveFeeRateAsync(FeeRate rate)
    {
        if (rate.Id == 0)
        {
            _context.FeeRates.Add(rate);
        }
        else
        {
            var existing = await _context.FeeRates.FindAsync(rate.Id);
            if (existing != null)
            {
                existing.StandardFee = rate.StandardFee;
                existing.LateFee = rate.LateFee;
                existing.IsActive = rate.IsActive;
                existing.UpdatedAt = DateTime.UtcNow;
                rate = existing;
            }
        }
        await _context.SaveChangesAsync();
        return rate;
    }

    public async Task<bool> DeleteFeeRateAsync(int id)
    {
        var item = await _context.FeeRates.FindAsync(id);
        if (item == null) return false;
        _context.FeeRates.Remove(item);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> BulkUpdateFeesAsync(int academicYearId, string feeType, decimal multiplierOrAddition, bool isPercentage)
    {
        var rates = await _context.FeeRates
            .Where(f => f.AcademicYearId == academicYearId && f.FeeType == feeType)
            .ToListAsync();

        foreach (var r in rates)
        {
            if (isPercentage)
            {
                r.StandardFee = Math.Round(r.StandardFee * (1 + multiplierOrAddition / 100m), 2);
                r.LateFee = Math.Round(r.LateFee * (1 + multiplierOrAddition / 100m), 2);
            }
            else
            {
                r.StandardFee += multiplierOrAddition;
                r.LateFee += multiplierOrAddition;
            }
            r.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> CopyRatesFromPreviousYearAsync(int sourceYearId, int targetYearId)
    {
        var sourceRates = await _context.FeeRates.Where(f => f.AcademicYearId == sourceYearId).ToListAsync();
        if (!sourceRates.Any()) return 0;

        // Clear existing target rates to avoid duplication
        var targetExisting = await _context.FeeRates.Where(f => f.AcademicYearId == targetYearId).ToListAsync();
        _context.FeeRates.RemoveRange(targetExisting);

        var newRates = sourceRates.Select(s => new FeeRate
        {
            AcademicYearId = targetYearId,
            FeeType = s.FeeType,
            ClassLevel = s.ClassLevel,
            GroupName = s.GroupName,
            StudentType = s.StudentType,
            FeeSlab = s.FeeSlab,
            StandardFee = s.StandardFee,
            LateFee = s.LateFee,
            IsActive = s.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        }).ToList();

        _context.FeeRates.AddRange(newRates);
        await _context.SaveChangesAsync();
        return newRates.Count;
    }
}
