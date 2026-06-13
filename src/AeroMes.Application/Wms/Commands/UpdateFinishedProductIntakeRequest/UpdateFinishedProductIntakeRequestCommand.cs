using AeroMes.Application.Common;
using AeroMes.Application.Wms.Commands.CreateFinishedProductIntakeRequest;
using AeroMes.Domain.Wms;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.UpdateFinishedProductIntakeRequest;

public record UpdateFinishedProductIntakeRequestCommand(
    int IntakeRequestId,
    IntakeRequestPurpose IntakePurpose,
    IntakeWarehouseType WarehouseType,
    int? ProductionOrderId,
    string RequesterUnit,
    DateTime RequestDate,
    string? Notes,
    IReadOnlyList<IntakeLineInput> Lines,
    string? UpdatedBy
) : ICommand<ValidationResult<Unit>>;
