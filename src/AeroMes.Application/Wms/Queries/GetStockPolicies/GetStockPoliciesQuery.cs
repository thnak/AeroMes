using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Wms.Queries.GetStockPolicies;

public record GetStockPoliciesQuery(bool? IsActive)
    : IQuery<IReadOnlyList<StockPolicyDto>>;

public record StockPolicyDto(
    int PolicyId,
    string ProductCode,
    int LocationId,
    decimal MinQty,
    decimal MaxQty,
    decimal SafetyStockQty,
    decimal ReorderQty,
    int LeadTimeDays,
    bool IsActive,
    string? CreatedBy,
    DateTime CreatedAt,
    string? UpdatedBy,
    DateTime? UpdatedAt);
