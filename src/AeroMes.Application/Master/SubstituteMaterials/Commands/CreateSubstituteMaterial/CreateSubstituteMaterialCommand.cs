using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.SubstituteMaterials.Commands.CreateSubstituteMaterial;

public record SubstituteMaterialCreatedResult(int SubstituteId, string SubstituteCode);

public record CreateSubstituteMaterialCommand(
    string PrimaryMaterialCode,
    string SubstituteMaterialCode,
    decimal ConversionRatio,
    int Priority,
    string? Notes,
    DateOnly? EffectiveDate,
    DateOnly? ExpiryDate,
    string? CreatedBy) : ICommand<ValidationResult<SubstituteMaterialCreatedResult>>;
