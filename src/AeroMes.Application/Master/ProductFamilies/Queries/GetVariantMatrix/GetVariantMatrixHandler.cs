using AeroMes.Domain.Master.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.ProductFamilies.Queries.GetVariantMatrix;

public sealed class GetVariantMatrixHandler(IProductFamilyRepository repo)
    : IQueryHandler<GetVariantMatrixQuery, VariantMatrixDto?>
{
    public async Task<VariantMatrixDto?> HandleAsync(GetVariantMatrixQuery query, CancellationToken ct)
    {
        var family = await repo.GetWithDimensionsAsync(query.FamilyCode.ToUpperInvariant(), ct);
        if (family is null) return null;

        var variants = await repo.GetVariantsAsync(family.FamilyCode, ct);

        return new VariantMatrixDto(
            family.FamilyCode,
            family.FamilyName,
            family.Industry,
            family.Dimensions.OrderBy(d => d.SortOrder).Select(d => new DimensionDto(
                d.DimensionID, d.DimensionName, d.SortOrder, d.IsRequired,
                d.Values.OrderBy(v => v.SortOrder).Select(v => new DimensionValueDto(
                    v.ValueID, v.ValueCode, v.ValueLabel, v.SortOrder, v.IsActive)).ToList()
            )).ToList(),
            variants.Select(v => new VariantRowDto(
                v.VariantID, v.ProductCode, v.VariantKey, v.VariantAttributes, v.IsActive)).ToList()
        );
    }
}
