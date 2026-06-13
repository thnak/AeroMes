using AeroMes.Domain.Production.Repositories;
using LiteBus.Queries.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.WorkOrders.Queries.GetWorkOrderDetail;

public class GetWorkOrderDetailHandler(
    IWorkOrderRepository workOrderRepo,
    IJobRepository jobRepo)
    : IQueryHandler<GetWorkOrderDetailQuery, QueryResult<WorkOrderDetailDto>>
{
    public async Task<QueryResult<WorkOrderDetailDto>> HandleAsync(GetWorkOrderDetailQuery q, CancellationToken ct)
    {
        var wo = await workOrderRepo.GetByIdAsync(q.Id, ct);
        if (wo is null) return QueryResult<WorkOrderDetailDto>.NotFound($"WorkOrder '{q.Id}' was not found.");

        var jobs = await jobRepo.GetByWoIdAsync(q.Id, ct);

        return QueryResult<WorkOrderDetailDto>.Found(new WorkOrderDetailDto(
            wo.WOID,
            wo.WOCode,
            wo.POID,
            wo.WorkCenterID,
            wo.WorkCenter?.WorkCenterName,
            wo.TargetQuantity.Value,
            wo.ActualQtyOK.Value,
            wo.ActualQtyNG.Value,
            wo.Status.ToString().ToUpperInvariant(),
            wo.ActualStartDate,
            wo.ActualEndDate,
            jobs.Select(j => new JobSummaryDto(
                j.JobID,
                j.MachineCode,
                j.ShiftCode,
                j.OperatorID,
                j.StartTime,
                j.EndTime,
                j.Status.ToString().ToUpperInvariant())).ToList()));
    }
}
