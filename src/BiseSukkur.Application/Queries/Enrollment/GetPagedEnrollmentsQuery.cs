using BiseSukkur.Core.Common;
using BiseSukkur.Core.DTOs;
using BiseSukkur.Core.Interfaces;
using MediatR;

namespace BiseSukkur.Application.Queries.Enrollment;

public record GetPagedEnrollmentsQuery(
    int SchoolId,
    PagedRequest Request
) : IRequest<Result<PagedResult<Core.Entities.Enrollment>>>;

public class GetPagedEnrollmentsQueryHandler : IRequestHandler<GetPagedEnrollmentsQuery, Result<PagedResult<Core.Entities.Enrollment>>>
{
    private readonly IEnrollmentService _enrollmentService;
    private readonly ICurrentUserService _currentUser;

    public GetPagedEnrollmentsQueryHandler(IEnrollmentService enrollmentService, ICurrentUserService currentUser)
    {
        _enrollmentService = enrollmentService;
        _currentUser = currentUser;
    }

    public async Task<Result<PagedResult<Core.Entities.Enrollment>>> Handle(GetPagedEnrollmentsQuery query, CancellationToken cancellationToken)
    {
        if (_currentUser.IsSchoolAdmin && _currentUser.SchoolId.HasValue && _currentUser.SchoolId.Value != query.SchoolId)
        {
            return Result.Failure<PagedResult<Core.Entities.Enrollment>>(Error.Unauthorized);
        }

        try
        {
            var paged = await _enrollmentService.GetPagedEnrollmentsAsync(query.SchoolId, query.Request);
            return Result.Success(paged);
        }
        catch (Exception ex)
        {
            return Result.Failure<PagedResult<Core.Entities.Enrollment>>("Enrollment.QueryFailed", ex.Message);
        }
    }
}
