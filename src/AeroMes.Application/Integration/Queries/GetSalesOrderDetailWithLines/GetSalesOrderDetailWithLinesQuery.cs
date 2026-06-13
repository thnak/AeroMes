using AeroMes.Application.Common;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Integration.Queries.GetSalesOrderDetailWithLines;

public record SoLineDto(
    int LineId,
    string ProductCode,
    string? ProductName,
    decimal Quantity,
    string? Unit,
    decimal UnitPrice);

public record SalesOrderWithLinesDto(
    int SOID,
    string SOCode,
    string? CustomerCode,
    string? CustomerName,
    DateTime OrderDate,
    DateTime? DeliveryDate,
    string Status,
    string SyncSource,
    string? FacilityCode,
    string? ConfirmedBy,
    DateTime? ConfirmedAt,
    string? Notes,
    string? CreatedBy,
    DateTime CreatedAt,
    IReadOnlyList<SoLineDto> Lines);

public record GetSalesOrderDetailWithLinesQuery(int SOID) : IQuery<QueryResult<SalesOrderWithLinesDto>>;
