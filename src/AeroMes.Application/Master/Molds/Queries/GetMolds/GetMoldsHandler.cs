using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.Molds.Queries.GetMolds;

public class GetMoldsHandler(IMoldRepository repo)
    : IQueryHandler<GetMoldsQuery, IReadOnlyList<MoldDto>>
{
    public async Task<IReadOnlyList<MoldDto>> HandleAsync(GetMoldsQuery query, CancellationToken ct)
    {
        var molds = await repo.GetAllAsync(
            query.ActiveOnly, query.Status, query.MachineCode, query.ProductCode, query.Search, ct);

        return molds.Select(ToDto).ToList();
    }

    internal static MoldDto ToDto(Mold m) => new(
        m.MoldId, m.MoldCode, m.MoldName,
        m.MoldType.ToString(), m.Material, m.Cavities,
        m.MaxShots, m.CurrentShots,
        UtilizationPercent(m.CurrentShots, m.MaxShots),
        m.IsPmDue,
        m.Status.ToString(),
        m.CurrentMachineCode, m.StorageLocation,
        m.ProductMappings.FirstOrDefault(x => x.IsDefault)?.ProductCode,
        m.IsActive);

    internal static decimal UtilizationPercent(long currentShots, long maxShots) =>
        maxShots <= 0 ? 0m : Math.Round(currentShots * 100m / maxShots, 1);
}
