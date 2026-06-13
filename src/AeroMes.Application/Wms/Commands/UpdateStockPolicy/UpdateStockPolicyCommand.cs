using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.UpdateStockPolicy;

public record UpdateStockPolicyCommand(
    int PolicyId,
    decimal MinQty,
    decimal MaxQty,
    decimal SafetyStockQty,
    decimal ReorderQty,
    int LeadTimeDays,
    bool IsActive,
    string? UpdatedBy
) : ICommand<ValidationResult<Unit>>;
