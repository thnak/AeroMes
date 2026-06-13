namespace AeroMes.Application.Master.SubstituteMaterials.Queries.GetSubstituteMaterials;

public record SubstituteMaterialDto(
    int SubstituteId,
    string SubstituteCode,
    string PrimaryMaterialCode,
    string? PrimaryMaterialName,
    string SubstituteMaterialCode,
    string? SubstituteMaterialName,
    decimal ConversionRatio,
    int Priority,
    string Status,
    string? Notes,
    DateOnly? EffectiveDate,
    DateOnly? ExpiryDate,
    DateTime CreatedAt);
