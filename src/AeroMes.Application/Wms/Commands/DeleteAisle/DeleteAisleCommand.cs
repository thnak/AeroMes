using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.DeleteAisle;

public record DeleteAisleCommand(int AisleId, string? DeletedBy) : ICommand<ValidationResult<Unit>>;
