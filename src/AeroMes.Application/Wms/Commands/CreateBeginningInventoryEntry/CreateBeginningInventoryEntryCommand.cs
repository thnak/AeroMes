using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.CreateBeginningInventoryEntry;

public record CreateBeginningInventoryEntryCommand(
    DateOnly Period,
    int WarehouseId,
    string ProductCode,
    string UnitOfMeasure,
    decimal BeginningQuantity,
    string? LotNumber,
    DateOnly? ExpirationDate,
    string? CreatedBy
) : ICommand<ValidationResult<BeginningInventoryEntryCreatedResult>>;

public record BeginningInventoryEntryCreatedResult(int EntryId);
