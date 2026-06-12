using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Tools.Commands.AddToolOperation;

public record AddToolOperationCommand(
    string ToolCode,
    string OperationCode,
    string? ProductCode,
    bool IsRequired,
    decimal UsageCountPerOp,
    string? UpdatedBy) : ICommand<int>;
