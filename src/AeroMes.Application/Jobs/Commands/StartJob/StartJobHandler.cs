using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using AeroMes.Domain.Production;
using AeroMes.Domain.Production.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Jobs.Commands.StartJob;

public class StartJobHandler(
    IWorkOrderRepository workOrderRepo,
    IMachineRepository machineRepo,
    IJobRepository jobRepo,
    IUnitOfWork uow)
    : ICommandHandler<StartJobCommand, StartJobResult>
{
    public async Task<StartJobResult> HandleAsync(StartJobCommand cmd, CancellationToken ct)
    {
        var workOrder = await workOrderRepo.GetByIdAsync(cmd.WorkOrderId, ct)
            ?? throw new EntityNotFoundException(nameof(WorkOrder), cmd.WorkOrderId);

        if (workOrder.Status != WorkOrderStatus.Running)
            throw new DomainException(
                $"WorkOrder '{workOrder.WOCode}' must be Running before starting a Job. Current: {workOrder.Status}.");

        if (!await machineRepo.ExistsAsync(cmd.MachineCode, ct))
            throw new EntityNotFoundException("Machine", cmd.MachineCode);

        var job = Job.Create(cmd.WorkOrderId, cmd.MachineCode, cmd.ShiftCode, cmd.OperatorId, cmd.StartTime);
        await jobRepo.AddAsync(job, ct);
        await uow.SaveChangesAsync(ct);

        return new StartJobResult(job.JobID, job.WOID, job.MachineCode,
            job.Status.ToString().ToUpperInvariant());
    }
}
