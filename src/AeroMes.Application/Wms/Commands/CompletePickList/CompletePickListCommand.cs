using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.CompletePickList;

public record CompletePickListCommand(
    int PickListId,
    string? CompletedBy) : ICommand<ValidationResult<Unit>>;
