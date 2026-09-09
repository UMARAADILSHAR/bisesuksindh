using BiseHyderabad.Application.Commands.Enrollment;
using BiseHyderabad.Application.Commands.Challan;
using BiseHyderabad.Application.Validators;
using BiseHyderabad.Core.DTOs;
using FluentAssertions;
using Xunit;

namespace BiseHyderabad.Core.Tests;

public class ValidationTests
{
    private readonly CreateEnrollmentValidator _enrollmentValidator = new();
    private readonly GenerateChallanValidator _challanValidator = new();

    [Fact]
    public void CreateEnrollmentValidator_FailsWhenCnicIsInvalid()
    {
        var command = new CreateEnrollmentCommand(
            SchoolId: 1,
            AcademicYearId: 1,
            StudentName: "Ali Khan",
            FatherName: "Nawaz Khan",
            Surname: "Khan",
            GrNumber: "GR-100",
            Cnic: "invalid-cnic",
            DateOfBirth: new DateTime(2008, 1, 1),
            Gender: "Male",
            Medium: "English",
            Religion: "Islam",
            Nationality: "Pakistani",
            ClassLevel: "SSC-I",
            Group: "Science",
            StudentType: "Regular",
            Subjects: new List<string> { "SINDHI", "ENGLISH-I", "ISLAMIAT", "PHYSICS-I", "CHEMISTRY-I" }
        );

        var result = _enrollmentValidator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Cnic");
    }

    [Fact]
    public void CreateEnrollmentValidator_PassesWithValidData()
    {
        var command = new CreateEnrollmentCommand(
            SchoolId: 1,
            AcademicYearId: 1,
            StudentName: "Ali Khan",
            FatherName: "Nawaz Khan",
            Surname: "Khan",
            GrNumber: "GR-100",
            Cnic: "41303-1234567-1",
            DateOfBirth: new DateTime(2008, 1, 1),
            Gender: "Male",
            Medium: "English",
            Religion: "Islam",
            Nationality: "Pakistani",
            ClassLevel: "SSC-I",
            Group: "Science",
            StudentType: "Regular",
            Subjects: new List<string> { "SINDHI", "ENGLISH-I", "ISLAMIAT", "PHYSICS-I", "CHEMISTRY-I" }
        );

        var result = _enrollmentValidator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void GenerateChallanValidator_FailsWhenNoStudentsSelected()
    {
        var req = new ChallanGenerationRequestDto
        {
            SchoolId = 1,
            AcademicYearId = 1,
            ChallanType = "enrollment",
            Students = new List<StudentSelectionDto>(),
            Username = "schooladmin"
        };

        var command = new GenerateChallanCommand(req);
        var result = _challanValidator.Validate(command);

        result.IsValid.Should().BeFalse();
    }
}
