using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Tools.Commands.CheckoutTool;

public record CheckoutToolCommand(
    string ToolCode,
    int WorkCenterId,
    string CheckedOutBy,
    DateTime? ExpectedReturnAt,
    string? UpdatedBy) : ICommand<ValidationResult<long>>;
