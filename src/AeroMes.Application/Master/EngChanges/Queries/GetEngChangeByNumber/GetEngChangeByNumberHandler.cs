using AeroMes.Application.Master.EngChanges.Queries.GetEngChanges;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.EngChanges.Queries.GetEngChangeByNumber;

public class GetEngChangeByNumberHandler(
    IEngChangeRepository repo,
    IBomHeaderRepository bomRepo)
    : IQueryHandler<GetEngChangeByNumberQuery, EngChangeDetailDto?>
{
    public async Task<EngChangeDetailDto?> HandleAsync(
        GetEngChangeByNumberQuery query, CancellationToken ct)
    {
        var ec = await repo.GetByNumberAsync(query.EcNumber, ct);
        if (ec is null) return null;

        string? bomProduct = null, bomVersion = null;
        if (ec.NewBomHeaderId is int bomHeaderId)
        {
            var header = await bomRepo.GetByIdAsync(bomHeaderId, ct);
            bomProduct = header?.ProductCode;
            bomVersion = header?.Version;
        }

        return new EngChangeDetailDto(
            GetEngChangesHandler.ToDto(ec), ec.Description, bomProduct, bomVersion);
    }
}
