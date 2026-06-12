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
    IEmployeeRepository employeeRepo,
    IWorkOrderAutoRulesRepository autoRulesRepo,
    IJobRepository jobRepo,
    IUnitOfWork uow)
    : ICommandHandler<StartJobCommand, StartJobResult>
{
    public async Task<StartJobResult> HandleAsync(StartJobCommand cmd, CancellationToken ct)
    {
        var workOrder = await workOrderRepo.GetByIdWithRoutingStepAsync(cmd.WorkOrderId, ct)
            ?? throw new EntityNotFoundException(nameof(WorkOrder), cmd.WorkOrderId);

        if (workOrder.Status != WorkOrderStatus.Running)
            throw new DomainException(
                $"WorkOrder '{workOrder.WOCode}' must be Running before starting a Job. Current: {workOrder.Status}.");

        if (!await machineRepo.ExistsAsync(cmd.MachineCode, ct))
            throw new EntityNotFoundException("Machine", cmd.MachineCode);

        await EnsureCertifiedAsync(workOrder, cmd.OperatorId, ct);

        var job = Job.Create(cmd.WorkOrderId, cmd.MachineCode, cmd.ShiftCode, cmd.OperatorId, cmd.StartTime);
        await jobRepo.AddAsync(job, ct);
        await uow.SaveChangesAsync(ct);

        return new StartJobResult(job.JobID, job.WOID, job.MachineCode,
            job.Status.ToString().ToUpperInvariant());
    }

    private async Task EnsureCertifiedAsync(WorkOrder workOrder, string operatorId, CancellationToken ct)
    {
        var rules = await autoRulesRepo.GetByWorkCenterAsync(workOrder.WorkCenterID, ct)
            ?? await autoRulesRepo.GetFactoryWideAsync(ct);
        if (rules is not { RequireCertification: true })
            return;

        var operationCode = workOrder.RoutingStep?.OperationCode;
        if (string.IsNullOrEmpty(operationCode))
            return;

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (!await employeeRepo.IsCertifiedAsync(operatorId, operationCode, today, ct))
            throw new DomainException(
                $"Operator '{operatorId}' is not certified for operation '{operationCode}' required by WorkOrder '{workOrder.WOCode}'.");
    }
}
