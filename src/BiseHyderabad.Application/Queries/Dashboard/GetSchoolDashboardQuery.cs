using BiseHyderabad.Core.Common;
using BiseHyderabad.Core.DTOs;
using BiseHyderabad.Core.Interfaces;
using MediatR;

namespace BiseHyderabad.Application.Queries.Dashboard;

public record GetSchoolDashboardQuery(int SchoolId) : IRequest<Result<SchoolDashboardStatsDto>>;

public class GetSchoolDashboardQueryHandler : IRequestHandler<GetSchoolDashboardQuery, Result<SchoolDashboardStatsDto>>
{
    private readonly IReportService _reportService;
    private readonly ICurrentUserService _currentUser;

    public GetSchoolDashboardQueryHandler(IReportService reportService, ICurrentUserService currentUser)
    {
        _reportService = reportService;
        _currentUser = currentUser;
    }

    public async Task<Result<SchoolDashboardStatsDto>> Handle(GetSchoolDashboardQuery request, CancellationToken cancellationToken)
    {
        if (_currentUser.IsSchoolAdmin && _currentUser.SchoolId.HasValue && _currentUser.SchoolId.Value != request.SchoolId)
        {
            return Result.Failure<SchoolDashboardStatsDto>(Error.Unauthorized);
        }

        try
        {
            var stats = await _reportService.GetSchoolDashboardStatsAsync(request.SchoolId);
            return Result.Success(stats);
        }
        catch (Exception ex)
        {
            return Result.Failure<SchoolDashboardStatsDto>("Dashboard.FetchFailed", ex.Message);
        }
    }
}
