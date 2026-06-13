using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.CreateGrn;

public record CreateGrnCommand(
    string GrnCode,
    int? PoId,
    int StorageLocationId,
    string ReceivedBy,
    DateTime ReceivedAt,
    string? DeliveryNoteRef,
    string? Notes,
    string? CreatedBy
) : ICommand<ValidationResult<GrnCreatedResult>>;

public record GrnCreatedResult(int GrnId, string GrnCode);
