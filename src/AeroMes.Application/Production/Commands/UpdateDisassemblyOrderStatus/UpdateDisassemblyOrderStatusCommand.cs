using AeroMes.Application.Common;
using AeroMes.Domain.Production;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.Commands.UpdateDisassemblyOrderStatus;

public record UpdateDisassemblyOrderStatusCommand(
    int DisassemblyOrderID,
    DisassemblyOrderStatus NewStatus) : ICommand<ValidationResult<Unit>>;
