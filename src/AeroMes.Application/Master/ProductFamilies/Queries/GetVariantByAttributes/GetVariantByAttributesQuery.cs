using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.ProductFamilies.Queries.GetVariantByAttributes;

public sealed record GetVariantByAttributesQuery(string FamilyCode, string VariantAttributesJson)
    : IQuery<VariantResolvedDto?>;

public sealed record VariantResolvedDto(string ProductCode, string VariantKey, string VariantAttributes);
