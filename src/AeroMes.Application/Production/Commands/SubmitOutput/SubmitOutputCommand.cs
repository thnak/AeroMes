using MediatR;

namespace AeroMes.Application.Production.Commands.SubmitOutput;

public record SubmitOutputCommand(
    int WorkOrderId,
    string OperatorId,
    string MachineCode,
    string ShiftCode,
    int QtyOk,
    int QtyNg,
    DateTime Timestamp,
    List<DefectEntry> Defects,
    string? IdempotencyKey = null
) : IRequest<SubmitOutputResult>;

public record DefectEntry(string DefectCode, int Qty);

public record SubmitOutputResult(long LogId, int CurrentWorkOrderActualOk, int CurrentWorkOrderActualNg);
