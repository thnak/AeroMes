using AeroMes.Domain.Master.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.UnitOfMeasures.Queries.GetAllUoMs;

public class GetAllUoMsHandler(IUnitOfMeasureRepository repo)
    : IQueryHandler<GetAllUoMsQuery, IReadOnlyList<UoMDto>>
{
    public async Task<IReadOnlyList<UoMDto>> HandleAsync(GetAllUoMsQuery q, CancellationToken ct)
    {
        var items = await repo.GetAllAsync(ct);
        return items.Select(x => new UoMDto(x.UoMCode, x.UoMName, x.UoMGroup)).ToList();
    }
}
