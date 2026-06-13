using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.ActivateBin;

public record ActivateBinCommand(int BinId, string UpdatedBy) : ICommand<ValidationResult<Unit>>;
