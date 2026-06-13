using System.Text.Json;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.ProductFamilies.Queries.GetVariantByAttributes;

public sealed class GetVariantByAttributesHandler(IProductFamilyRepository repo)
    : IQueryHandler<GetVariantByAttributesQuery, VariantResolvedDto?>
{
    public async Task<VariantResolvedDto?> HandleAsync(GetVariantByAttributesQuery query, CancellationToken ct)
    {
        var family = await repo.GetWithDimensionsAsync(query.FamilyCode.ToUpperInvariant(), ct);
        if (family is null) return null;

        // Build the variant key from the JSON attributes in dimension sort order
        Dictionary<string, string>? attrs;
        try { attrs = JsonSerializer.Deserialize<Dictionary<string, string>>(query.VariantAttributesJson); }
        catch { return null; }
        if (attrs is null) return null;

        var orderedDimensions = family.Dimensions.OrderBy(d => d.SortOrder).ToList();
        var keyParts = orderedDimensions
            .Where(d => d.IsRequired && attrs.ContainsKey(d.DimensionName))
            .Select(d => attrs[d.DimensionName]);
        var variantKey = string.Join("|", keyParts);

        var variant = await repo.GetVariantByKeyAsync(family.FamilyCode, variantKey, ct);
        if (variant is null) return null;

        return new VariantResolvedDto(variant.ProductCode, variant.VariantKey, variant.VariantAttributes);
    }
}
