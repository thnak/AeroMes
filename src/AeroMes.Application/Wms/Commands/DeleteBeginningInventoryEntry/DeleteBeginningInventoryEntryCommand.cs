using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.DeleteBeginningInventoryEntry;

public record DeleteBeginningInventoryEntryCommand(int EntryId, string? DeletedBy)
    : ICommand<ValidationResult<Unit>>;
