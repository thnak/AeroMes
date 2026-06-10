using AeroMes.Domain.Master.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.DowntimeReasonCodes.Queries.GetDowntimeReasonCodes;

public class GetDowntimeReasonCodesHandler(IDowntimeReasonCodeRepository repo)
    : IQueryHandler<GetDowntimeReasonCodesQuery, IReadOnlyList<DowntimeReasonCodeDto>>
{
    public async Task<IReadOnlyList<DowntimeReasonCodeDto>> HandleAsync(GetDowntimeReasonCodesQuery q, CancellationToken ct)
    {
        var items = await repo.GetAllAsync(q.ActiveOnly, ct);
        return items.Select(x => new DowntimeReasonCodeDto(
            x.ReasonCode, x.ReasonName, x.Category,
            x.SlaMinutes, x.RequiresApproval, x.IsActive)).ToList();
    }
}
