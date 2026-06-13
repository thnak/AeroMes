using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.DeactivateBin;

public record DeactivateBinCommand(int BinId, string UpdatedBy) : ICommand<ValidationResult<Unit>>;
