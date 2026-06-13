using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.RecordCycleCountLine;

public record RecordCycleCountLineCommand(
    int PlanId,
    long LineId,
    decimal CountedQty,
    string? CountedBy
) : ICommand<ValidationResult<Unit>>;
