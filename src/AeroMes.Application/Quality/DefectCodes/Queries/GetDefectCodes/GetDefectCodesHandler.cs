using AeroMes.Domain.Quality.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Quality.DefectCodes.Queries.GetDefectCodes;

public class GetDefectCodesHandler(IDefectCodeRepository repo)
    : IQueryHandler<GetDefectCodesQuery, IReadOnlyList<DefectCodeDto>>
{
    public async Task<IReadOnlyList<DefectCodeDto>> HandleAsync(GetDefectCodesQuery q, CancellationToken ct)
    {
        var items = await repo.GetAllAsync(q.ActiveOnly, ct);
        return items.Select(x => new DefectCodeDto(
            x.DefectCodeID, x.Code, x.DefectName, x.DefectCategory, x.IsActive)).ToList();
    }
}
