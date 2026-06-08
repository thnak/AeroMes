using AeroMes.Application.Interfaces;
using AeroMes.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Application.WorkOrders.Queries.GetWorkOrders;

public class GetWorkOrdersHandler(IApplicationDbContext db)
    : IRequestHandler<GetWorkOrdersQuery, List<WorkOrderDto>>
{
    public async Task<List<WorkOrderDto>> Handle(GetWorkOrdersQuery query, CancellationToken ct)
    {
        var q = db.WorkOrders.Include(x => x.WorkCenter).AsNoTracking();

        if (!string.IsNullOrEmpty(query.Status) &&
            Enum.TryParse<WorkOrderStatus>(query.Status, ignoreCase: true, out var status))
            q = q.Where(x => x.Status == status);

        if (query.WorkCenterId.HasValue)
            q = q.Where(x => x.WorkCenterID == query.WorkCenterId.Value);

        return await q.Select(x => new WorkOrderDto(
            x.WorkOrderID,
            x.WorkOrderNo,
            x.ProductCode,
            x.ProductName,
            x.TargetQuantity,
            x.ActualQtyOK,
            x.ActualQtyNG,
            x.Status.ToString().ToUpperInvariant(),
            x.WorkCenter.WorkCenterCode
        )).ToListAsync(ct);
    }
}
