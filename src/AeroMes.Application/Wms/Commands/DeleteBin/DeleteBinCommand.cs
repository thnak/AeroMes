using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.DeleteBin;

public record DeleteBinCommand(int BinId, string? DeletedBy) : ICommand<ValidationResult<Unit>>;
