using AeroMes.Application.Common;
using AeroMes.Domain.Wms;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.CreateShipmentOrder;

public record CreateShipmentOrderCommand(
    int? SoId,
    string CustomerName,
    DateOnly RequestedShipDate,
    string? CreatedBy) : ICommand<ValidationResult<ShipmentCreatedResult>>;

public record ShipmentCreatedResult(int ShipmentId, string ShipmentCode);
