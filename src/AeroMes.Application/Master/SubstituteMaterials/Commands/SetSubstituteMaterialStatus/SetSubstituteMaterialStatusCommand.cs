using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.SubstituteMaterials.Commands.SetSubstituteMaterialStatus;

public record SetSubstituteMaterialStatusCommand(
    int SubstituteId,
    bool IsActive,
    string? UpdatedBy) : ICommand<ValidationResult<Unit>>;
