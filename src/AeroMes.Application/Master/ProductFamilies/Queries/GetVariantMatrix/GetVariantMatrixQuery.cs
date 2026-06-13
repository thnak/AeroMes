using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.ProductFamilies.Queries.GetVariantMatrix;

public sealed record GetVariantMatrixQuery(string FamilyCode)
    : IQuery<VariantMatrixDto?>;

public sealed record VariantMatrixDto(
    string FamilyCode,
    string FamilyName,
    string Industry,
    IReadOnlyList<DimensionDto> Dimensions,
    IReadOnlyList<VariantRowDto> Variants);

public sealed record DimensionDto(
    int DimensionId,
    string DimensionName,
    int SortOrder,
    bool IsRequired,
    IReadOnlyList<DimensionValueDto> Values);

public sealed record DimensionValueDto(
    int ValueId,
    string ValueCode,
    string ValueLabel,
    int SortOrder,
    bool IsActive);

public sealed record VariantRowDto(
    int VariantId,
    string ProductCode,
    string VariantKey,
    string VariantAttributes,
    bool IsActive);
