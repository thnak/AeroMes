using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.SubstituteMaterials.Queries.GetSubstituteMaterials;

public class GetSubstituteMaterialsHandler(ISubstituteMaterialRepository repo)
    : IQueryHandler<GetSubstituteMaterialsQuery, IReadOnlyList<SubstituteMaterialDto>>
{
    public async Task<IReadOnlyList<SubstituteMaterialDto>> HandleAsync(
        GetSubstituteMaterialsQuery query, CancellationToken ct)
    {
        IReadOnlyList<SubstituteMaterial> items;

        if (query.PrimaryMaterialCode is not null)
        {
            items = await repo.GetByPrimaryMaterialAsync(query.PrimaryMaterialCode, ct);
            if (query.ActiveOnly == true)
                items = items.Where(x => x.Status == SubstituteMaterialStatus.Active).ToList();
        }
        else
        {
            var status = query.ActiveOnly == true ? SubstituteMaterialStatus.Active : (SubstituteMaterialStatus?)null;
            items = await repo.GetAllAsync(null, status, ct);
        }

        return items
            .Select(x => new SubstituteMaterialDto(
                x.SubstituteId,
                x.SubstituteCode,
                x.PrimaryMaterialCode,
                x.PrimaryMaterial?.ProductName,
                x.SubstituteMaterialCode,
                x.SubstituteMaterialProduct?.ProductName,
                x.ConversionRatio,
                x.Priority,
                x.Status.ToString(),
                x.Notes,
                x.EffectiveDate,
                x.ExpiryDate,
                x.CreatedAt))
            .ToList();
    }
}
