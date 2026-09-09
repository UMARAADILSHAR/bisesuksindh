using BiseSukkur.Core.Common;
using BiseSukkur.Core.Interfaces;
using MediatR;

namespace BiseSukkur.Application.Commands.Examination;

public record AllotRollNumbersCommand(
    int AcademicYearId,
    string ClassLevel,
    string AllocatedByUsername
) : IRequest<Result<int>>;

public class AllotRollNumbersCommandHandler : IRequestHandler<AllotRollNumbersCommand, Result<int>>
{
    private readonly IExaminationService _examService;
    private readonly ICurrentUserService _currentUser;

    public AllotRollNumbersCommandHandler(IExaminationService examService, ICurrentUserService currentUser)
    {
        _examService = examService;
        _currentUser = currentUser;
    }

    public async Task<Result<int>> Handle(AllotRollNumbersCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsSuperAdmin)
        {
            return Result.Failure<int>(Error.Unauthorized);
        }

        try
        {
            var count = await _examService.RunRollNumberAllotmentAsync(
                request.AcademicYearId,
                request.ClassLevel
            );

            return Result.Success(count);
        }
        catch (Exception ex)
        {
            return Result.Failure<int>("Examination.AllotmentFailed", ex.Message);
        }
    }
}
