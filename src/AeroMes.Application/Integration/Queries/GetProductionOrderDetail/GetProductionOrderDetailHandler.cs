using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Integration.Repositories;
using AeroMes.Domain.Production.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Integration.Queries.GetProductionOrderDetail;

public class GetProductionOrderDetailHandler(
    IProductionOrderRepository poRepo,
    IWorkOrderRepository woRepo)
    : IQueryHandler<GetProductionOrderDetailQuery, ProductionOrderDetailDto>
{
    public async Task<ProductionOrderDetailDto> HandleAsync(
        GetProductionOrderDetailQuery q, CancellationToken ct)
    {
        var po = await poRepo.GetByIdAsync(q.Id, ct)
            ?? throw new EntityNotFoundException("ProductionOrder", q.Id);

        var wos = await woRepo.GetByPoIdAsync(q.Id, ct);

        return new ProductionOrderDetailDto(
            po.POID, po.POCode, po.SOID, po.ProductCode, po.TargetQuantity,
            po.Status.ToString(), po.PlannedStartDate, po.PlannedEndDate,
            po.ActualStartDate, po.ActualEndDate, po.SyncedAt,
            [.. wos.Select(w => new WorkOrderSummaryDto(
                w.WOID, w.WOCode, w.WorkCenterID, w.WorkCenter?.WorkCenterName,
                w.TargetQuantity.Value, w.ActualQtyOK.Value, w.ActualQtyNG.Value,
                w.Status.ToString()))]);
    }
}
