using BiseHyderabad.Core.Common;
using BiseHyderabad.Core.Interfaces;
using MediatR;

namespace BiseHyderabad.Application.Commands.Enrollment;

public record CreateEnrollmentCommand(
    int SchoolId,
    int AcademicYearId,
    string StudentName,
    string FatherName,
    string? Surname,
    string GrNumber,
    string Cnic,
    DateTime DateOfBirth,
    string Gender,
    string Medium,
    string Religion,
    string Nationality,
    string ClassLevel,
    string Group,
    string StudentType,
    List<string> Subjects,
    string? MobileNumber = null,
    string? Address = null
) : IRequest<Result<Core.Entities.Enrollment>>;

public class CreateEnrollmentCommandHandler : IRequestHandler<CreateEnrollmentCommand, Result<Core.Entities.Enrollment>>
{
    private readonly IEnrollmentService _enrollmentService;
    private readonly ICurrentUserService _currentUser;

    public CreateEnrollmentCommandHandler(IEnrollmentService enrollmentService, ICurrentUserService currentUser)
    {
        _enrollmentService = enrollmentService;
        _currentUser = currentUser;
    }

    public async Task<Result<Core.Entities.Enrollment>> Handle(CreateEnrollmentCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.IsSchoolAdmin && _currentUser.SchoolId.HasValue && _currentUser.SchoolId.Value != request.SchoolId)
        {
            return Result.Failure<Core.Entities.Enrollment>(Error.Unauthorized);
        }

        try
        {
            var enrollment = new Core.Entities.Enrollment
            {
                SchoolId = request.SchoolId,
                AcademicYearId = request.AcademicYearId,
                StudentName = request.StudentName,
                FatherName = request.FatherName,
                Surname = request.Surname,
                GrNumber = request.GrNumber,
                Cnic = request.Cnic,
                DateOfBirth = request.DateOfBirth,
                Gender = request.Gender,
                Medium = request.Medium,
                Religion = request.Religion,
                Nationality = request.Nationality,
                ClassLevel = request.ClassLevel,
                Group = request.Group,
                StudentType = request.StudentType,
                SubjectsJson = System.Text.Json.JsonSerializer.Serialize(request.Subjects),
                MobileNumber = request.MobileNumber,
                Address = request.Address
            };

            var saved = await _enrollmentService.SaveEnrollmentAsync(enrollment);
            return Result.Success(saved);
        }
        catch (Exception ex)
        {
            return Result.Failure<Core.Entities.Enrollment>("Enrollment.CreateFailed", ex.Message);
        }
    }
}
