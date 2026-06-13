using AeroMes.Application.Common;
using AeroMes.Domain.Wms;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.CreateBin;

public record CreateBinCommand(
    int RackId,
    string BinCode,
    string BinLevel,
    BinType BinType,
    decimal? MaxQty,
    string? CreatedBy)
    : ICommand<ValidationResult<int>>;
