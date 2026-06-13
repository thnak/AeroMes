using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.CreateStockPolicy;

public record CreateStockPolicyCommand(
    string ProductCode,
    int LocationId,
    decimal MinQty,
    decimal MaxQty,
    decimal SafetyStockQty,
    decimal ReorderQty,
    int LeadTimeDays,
    string? CreatedBy
) : ICommand<ValidationResult<StockPolicyCreatedResult>>;

public record StockPolicyCreatedResult(int PolicyId);
