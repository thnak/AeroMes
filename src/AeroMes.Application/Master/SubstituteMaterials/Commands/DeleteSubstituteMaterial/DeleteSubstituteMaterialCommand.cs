using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.SubstituteMaterials.Commands.DeleteSubstituteMaterial;

public record DeleteSubstituteMaterialCommand(
    int SubstituteId,
    string? DeletedBy) : ICommand<ValidationResult<Unit>>;
