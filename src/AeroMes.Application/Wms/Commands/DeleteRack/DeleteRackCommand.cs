using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.DeleteRack;

public record DeleteRackCommand(int RackId, string? DeletedBy) : ICommand<ValidationResult<Unit>>;
