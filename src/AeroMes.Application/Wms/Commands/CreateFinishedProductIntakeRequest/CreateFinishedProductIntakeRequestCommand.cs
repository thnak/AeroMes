using AeroMes.Application.Common;
using AeroMes.Domain.Wms;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.CreateFinishedProductIntakeRequest;

public record CreateFinishedProductIntakeRequestCommand(
    IntakeRequestPurpose IntakePurpose,
    IntakeWarehouseType WarehouseType,
    int? ProductionOrderId,
    string RequesterUnit,
    DateTime RequestDate,
    string? Notes,
    IReadOnlyList<IntakeLineInput> Lines,
    string? CreatedBy
) : ICommand<ValidationResult<FinishedProductIntakeRequestCreatedResult>>;

public record IntakeLineInput(
    string ProductCode,
    string UnitOfMeasure,
    decimal RequestedQuantity,
    int WarehouseId,
    bool IsDefective,
    string? DefectReason,
    string? Notes);

public record FinishedProductIntakeRequestCreatedResult(int IntakeRequestId, string RequestNumber);
