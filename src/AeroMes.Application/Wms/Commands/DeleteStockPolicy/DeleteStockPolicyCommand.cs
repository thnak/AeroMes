using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.DeleteStockPolicy;

public record DeleteStockPolicyCommand(int PolicyId, string? DeletedBy)
    : ICommand<ValidationResult<Unit>>;
