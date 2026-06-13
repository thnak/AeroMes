using AeroMes.Application.Master.SubstituteMaterials.Queries.GetSubstituteMaterials;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.SubstituteMaterials.Queries.GetSubstituteMaterialById;

public class GetSubstituteMaterialByIdHandler(ISubstituteMaterialRepository repo)
    : IQueryHandler<GetSubstituteMaterialByIdQuery, SubstituteMaterialDto?>
{
    public async Task<SubstituteMaterialDto?> HandleAsync(GetSubstituteMaterialByIdQuery query, CancellationToken ct)
    {
        var entity = await repo.GetByIdAsync(query.SubstituteId, ct);
        if (entity is null) return null;

        return new SubstituteMaterialDto(
            entity.SubstituteId,
            entity.SubstituteCode,
            entity.PrimaryMaterialCode,
            entity.PrimaryMaterial?.ProductName,
            entity.SubstituteMaterialCode,
            entity.SubstituteMaterialProduct?.ProductName,
            entity.ConversionRatio,
            entity.Priority,
            entity.Status.ToString(),
            entity.Notes,
            entity.EffectiveDate,
            entity.ExpiryDate,
            entity.CreatedAt);
    }
}
