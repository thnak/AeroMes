using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.Commands.SubmitOutput;

public record SubmitOutputCommand(
    long JobId,
    int QtyOk,
    int QtyNg,
    string? DeviceIp,
    string? Notes,
    string? IdempotencyKey,
    DateTime? Timestamp,
    List<DefectEntry> Defects) : ICommand<ValidationResult<SubmitOutputResult>>;

public record DefectEntry(string DefectCode, int Qty);

public record SubmitOutputResult(long LogId, int WorkOrderOK, int WorkOrderNG, bool IsDuplicate = false);
