using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.CreatePickList;

public record CreatePickListCommand(
    int ShipmentId,
    int LocationId,
    string? AssignedTo,
    string? CreatedBy) : ICommand<ValidationResult<PickListCreatedResult>>;

public record PickListCreatedResult(int PickListId, int TotalLines, bool IsFullyAllocated);
