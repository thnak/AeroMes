using AeroMes.Application.Common;
using AeroMes.Domain.Wms;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.UpdateBin;

public record UpdateBinCommand(int BinId, string BinLevel, BinType BinType, decimal? MaxQty, string UpdatedBy)
    : ICommand<ValidationResult<Unit>>;
