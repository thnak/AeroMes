using AeroMes.Domain.Master.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.Molds.Queries.GetMoldsDueForPm;

public class GetMoldsDueForPmHandler(IMoldRepository repo)
    : IQueryHandler<GetMoldsDueForPmQuery, IReadOnlyList<MoldPmDueDto>>
{
    public async Task<IReadOnlyList<MoldPmDueDto>> HandleAsync(
        GetMoldsDueForPmQuery query, CancellationToken ct)
    {
        // 0.9 — surfaces molds approaching the PM interval, not only overdue ones.
        var molds = await repo.GetDueForPmAsync(0.9, ct);

        return molds
            .Select(m => new MoldPmDueDto(
                m.MoldId, m.MoldCode, m.MoldName, m.Status.ToString(),
                m.CurrentShots, m.ShotsAtLastPm, m.PmIntervalShots,
                m.CurrentShots - m.ShotsAtLastPm,
                m.IsPmDue))
            .ToList();
    }
}
