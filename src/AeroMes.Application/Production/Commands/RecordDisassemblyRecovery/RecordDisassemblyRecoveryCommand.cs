using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.Commands.RecordDisassemblyRecovery;

public record RecordDisassemblyRecoveryCommand(
    int DisassemblyOrderID,
    string ProductCode,
    decimal ActualQty) : ICommand<ValidationResult<Unit>>;
