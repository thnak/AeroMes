using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.SubstituteMaterials.Commands.UpdateSubstituteMaterial;

public record UpdateSubstituteMaterialCommand(
    int SubstituteId,
    decimal ConversionRatio,
    int Priority,
    string? Notes,
    DateOnly? EffectiveDate,
    DateOnly? ExpiryDate,
    string? UpdatedBy) : ICommand<ValidationResult<Unit>>;
