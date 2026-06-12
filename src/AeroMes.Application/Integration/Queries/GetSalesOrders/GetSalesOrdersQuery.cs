using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Integration.Queries.GetSalesOrders;

public record GetSalesOrdersQuery(
    string? SoCode,
    string? Status,
    DateTime? From,
    DateTime? To) : IQuery<IReadOnlyList<SalesOrderDto>>;

public record SalesOrderDto(
    int SOID,
    string SOCode,
    string? CustomerName,
    DateTime OrderDate,
    DateTime? DeliveryDate,
    string Status,
    DateTime SyncedAt);
