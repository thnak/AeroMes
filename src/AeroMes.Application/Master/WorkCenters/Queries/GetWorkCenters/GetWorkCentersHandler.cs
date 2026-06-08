using AeroMes.Domain.Master.Repositories;
using MediatR;

namespace AeroMes.Application.Master.WorkCenters.Queries.GetWorkCenters;

public class GetWorkCentersHandler(IWorkCenterRepository repo)
    : IRequestHandler<GetWorkCentersQuery, IReadOnlyList<WorkCenterDto>>
{
    public async Task<IReadOnlyList<WorkCenterDto>> Handle(GetWorkCentersQuery q, CancellationToken ct)
    {
        var items = await repo.GetAllAsync(q.ActiveOnly, ct);
        return items.Select(x => new WorkCenterDto(
            x.WorkCenterID,
            x.WorkCenterCode,
            x.WorkCenterName,
            x.Description,
            x.IsActive)).ToList();
    }
}
