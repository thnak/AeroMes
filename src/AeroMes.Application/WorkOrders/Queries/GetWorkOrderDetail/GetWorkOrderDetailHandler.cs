using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Production.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.WorkOrders.Queries.GetWorkOrderDetail;

public class GetWorkOrderDetailHandler(
    IWorkOrderRepository workOrderRepo,
    IJobRepository jobRepo)
    : IQueryHandler<GetWorkOrderDetailQuery, WorkOrderDetailDto>
{
    public async Task<WorkOrderDetailDto> HandleAsync(GetWorkOrderDetailQuery q, CancellationToken ct)
    {
        var wo = await workOrderRepo.GetByIdAsync(q.Id, ct)
            ?? throw new EntityNotFoundException("WorkOrder", q.Id);

        var jobs = await jobRepo.GetByWoIdAsync(q.Id, ct);

        return new WorkOrderDetailDto(
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
                j.Status.ToString().ToUpperInvariant())).ToList());
    }
}
