using AeroMes.Application.Common;
using AeroMes.Domain.Integration.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Integration.Queries.GetSalesOrderDetailWithLines;

public class GetSalesOrderDetailWithLinesHandler(
    ISalesOrderRepository repo) : IQueryHandler<GetSalesOrderDetailWithLinesQuery, QueryResult<SalesOrderWithLinesDto>>
{
    public async Task<QueryResult<SalesOrderWithLinesDto>> HandleAsync(
        GetSalesOrderDetailWithLinesQuery query, CancellationToken ct = default)
    {
        var so = await repo.GetByIdWithLinesAsync(query.SOID, ct);
        if (so is null)
            return QueryResult<SalesOrderWithLinesDto>.NotFound($"Sales order {query.SOID} not found.");

        var lines = so.Lines.Select(l => new SoLineDto(
            l.LineId, l.ProductCode, l.ProductName,
            l.Quantity, l.Unit, l.UnitPrice)).ToList();

        var dto = new SalesOrderWithLinesDto(
            so.SOID, so.SOCode, so.CustomerCode, so.CustomerName,
            so.OrderDate, so.DeliveryDate, so.Status.ToString(),
            so.SyncSource.ToString(), so.FacilityCode,
            so.ConfirmedBy, so.ConfirmedAt, so.Notes,
            so.CreatedBy, so.CreatedAt, lines);

        return QueryResult<SalesOrderWithLinesDto>.Found(dto);
    }
}
