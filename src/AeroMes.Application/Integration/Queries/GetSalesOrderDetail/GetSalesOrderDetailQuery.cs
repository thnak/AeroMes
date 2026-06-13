using LiteBus.Queries.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Integration.Queries.GetSalesOrderDetail;

public record GetSalesOrderDetailQuery(int Id) : IQuery<QueryResult<SalesOrderDetailDto>>;

public record SalesOrderDetailDto(
    int SOID,
    string SOCode,
    string? CustomerName,
    DateTime OrderDate,
    DateTime? DeliveryDate,
    string Status,
    DateTime SyncedAt,
    IReadOnlyList<ProductionOrderSummaryDto> ProductionOrders);

public record ProductionOrderSummaryDto(
    int POID,
    string POCode,
    string ProductCode,
    int TargetQuantity,
    string Status,
    DateTime? PlannedStartDate,
    DateTime? PlannedEndDate);
