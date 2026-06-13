using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.UpdateBeginningInventoryEntry;

public record UpdateBeginningInventoryEntryCommand(
    int EntryId,
    decimal BeginningQuantity,
    string? LotNumber,
    DateOnly? ExpirationDate,
    string? UpdatedBy
) : ICommand<ValidationResult<Unit>>;
