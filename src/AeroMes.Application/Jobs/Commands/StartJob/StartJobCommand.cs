using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Jobs.Commands.StartJob;

public record StartJobCommand(
    int WorkOrderId,
    string MachineCode,
    string ShiftCode,
    string OperatorId,
    DateTime? StartTime = null) : ICommand<StartJobResult>;

public record StartJobResult(long JobID, int WOID, string MachineCode, string Status);
