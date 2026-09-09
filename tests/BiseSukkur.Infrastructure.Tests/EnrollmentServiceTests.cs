using BiseSukkur.Core.DTOs;
using BiseSukkur.Core.Entities;
using BiseSukkur.Core.Enums;
using BiseSukkur.Core.Interfaces;
using BiseSukkur.Infrastructure.Data;
using BiseSukkur.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace BiseSukkur.Infrastructure.Tests;

public class EnrollmentServiceTests
{
    private ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task GetPagedEnrollmentsAsync_CorrectlyPagesAndFiltersResults()
    {
        using var context = CreateDbContext();
        var mockActivityLog = new Mock<IActivityLogService>();
        var service = new EnrollmentService(context, mockActivityLog.Object);

        // Seed School & AcademicYear
        var school = new School { Id = 1, Code = "001", Name = "Test School", SemisCode = "418030045" };
        var year = new AcademicYear { Id = 1, YearName = "2026", IsActive = true };
        context.Schools.Add(school);
        context.AcademicYears.Add(year);

        // Seed 10 enrollments
        for (int i = 1; i <= 10; i++)
        {
            context.Enrollments.Add(new Enrollment
            {
                SchoolId = 1,
                AcademicYearId = 1,
                StudentName = $"Candidate {i:D2}",
                FatherName = $"Father {i:D2}",
                GrNumber = $"GR-{i:D3}",
                Cnic = $"41303-1234567-{i}",
                ClassLevel = i <= 6 ? "SSC-I" : "HSC-I",
                Group = "Science",
                Status = EnrollmentStatus.Final
            });
        }
        await context.SaveChangesAsync();

        // Request Page 1 with PageSize 4
        var request = new PagedRequest { PageNumber = 1, PageSize = 4 };
        var result = await service.GetPagedEnrollmentsAsync(1, request);

        result.TotalCount.Should().Be(10);
        result.Items.Should().HaveCount(4);
        result.TotalPages.Should().Be(3);
        result.HasNextPage.Should().BeTrue();
        result.HasPreviousPage.Should().BeFalse();

        // Filter by class
        request.Filters["classLevel"] = "HSC-I";
        var hscResult = await service.GetPagedEnrollmentsAsync(1, request);
        hscResult.TotalCount.Should().Be(4);
        hscResult.Items.Should().HaveCount(4);
    }

    [Fact]
    public async Task GetEnrollmentMetricsAsync_CalculatesAccurateCounts()
    {
        using var context = CreateDbContext();
        var mockActivityLog = new Mock<IActivityLogService>();
        var service = new EnrollmentService(context, mockActivityLog.Object);

        context.Enrollments.AddRange(
            new Enrollment { SchoolId = 1, AcademicYearId = 1, StudentName = "Student 1", Cnic = "111", ClassLevel = "SSC-I", EnrollmentNumber = "E26-001" },
            new Enrollment { SchoolId = 1, AcademicYearId = 1, StudentName = "Student 2", Cnic = "222", ClassLevel = "SSC-II" },
            new Enrollment { SchoolId = 1, AcademicYearId = 1, StudentName = "Student 3", Cnic = "333", ClassLevel = "HSC-I" }
        );
        await context.SaveChangesAsync();

        var metrics = await service.GetEnrollmentMetricsAsync(1, 1);

        metrics.TotalEnrolled.Should().Be(3);
        metrics.SscCount.Should().Be(2);
        metrics.HscCount.Should().Be(1);
        metrics.AllottedCount.Should().Be(1);
    }
}
