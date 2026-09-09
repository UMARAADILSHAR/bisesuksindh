using BiseSukkur.Core.Common;
using BiseSukkur.Core.DTOs;
using BiseSukkur.Core.Interfaces;
using MediatR;

namespace BiseSukkur.Application.Commands.Challan;

public record GenerateChallanCommand(ChallanGenerationRequestDto Request) : IRequest<Result<ChallanGenerationResultDto>>;

public class GenerateChallanCommandHandler : IRequestHandler<GenerateChallanCommand, Result<ChallanGenerationResultDto>>
{
    private readonly IChallanService _challanService;
    private readonly ICurrentUserService _currentUser;

    public GenerateChallanCommandHandler(IChallanService challanService, ICurrentUserService currentUser)
    {
        _challanService = challanService;
        _currentUser = currentUser;
    }

    public async Task<Result<ChallanGenerationResultDto>> Handle(GenerateChallanCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        if (_currentUser.IsSchoolAdmin && _currentUser.SchoolId.HasValue && _currentUser.SchoolId.Value != request.SchoolId)
        {
            return Result.Failure<ChallanGenerationResultDto>(Error.Unauthorized);
        }

        if (request.Students == null || !request.Students.Any(s => s.IsIncluded))
        {
            return Result.Failure<ChallanGenerationResultDto>("Challan.NoItems", "At least one candidate must be selected for challan generation.");
        }

        try
        {
            var result = await _challanService.GenerateChallanAsync(request);
            if (!result.Success)
            {
                return Result.Failure<ChallanGenerationResultDto>("Challan.GenerationFailed", result.ErrorMessage ?? "Challan generation failed.");
            }

            return Result.Success(result);
        }
        catch (Exception ex)
        {
            return Result.Failure<ChallanGenerationResultDto>("Challan.GenerationFailed", ex.Message);
        }
    }
}
