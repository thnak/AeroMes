using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master.Repositories;
using AeroMes.Domain.Production;
using AeroMes.Domain.Production.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Application.Jobs.Commands.StartJob;

public class StartJobHandler(
    IWorkOrderRepository workOrderRepo,
    IMachineRepository machineRepo,
    IEmployeeRepository employeeRepo,
    IWorkOrderAutoRulesRepository autoRulesRepo,
    IJobRepository jobRepo,
    IUnitOfWork uow,
    IValidator<StartJobCommand> validator)
    : ICommandHandler<StartJobCommand, ValidationResult<StartJobResult>>
{
    public async Task<ValidationResult<StartJobResult>> HandleAsync(StartJobCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<StartJobResult>.Invalid(validation.ToErrorDictionary());

        try
        {
            var workOrder = await workOrderRepo.GetByIdWithRoutingStepAsync(cmd.WorkOrderId, ct);
            if (workOrder is null) return ValidationResult<StartJobResult>.NotFound($"Entity '{cmd.WorkOrderId}' was not found.");

            if (workOrder.Status != WorkOrderStatus.Running)
                throw new DomainException(
                    $"WorkOrder '{workOrder.WOCode}' must be Running before starting a Job. Current: {workOrder.Status}.");

            if (!await machineRepo.ExistsAsync(cmd.MachineCode, ct))
                return ValidationResult<StartJobResult>.NotFound($"Machine '{cmd.MachineCode}' was not found.");

            await EnsureCertifiedAsync(workOrder, cmd.OperatorId, ct);

            var job = Job.Create(cmd.WorkOrderId, cmd.MachineCode, cmd.ShiftCode, cmd.OperatorId, cmd.StartTime);
            await jobRepo.AddAsync(job, ct);
            await uow.SaveChangesAsync(ct);

            return ValidationResult<StartJobResult>.Ok(new StartJobResult(job.JobID, job.WOID, job.MachineCode,
                job.Status.ToString().ToUpperInvariant()));
        }        catch (DomainException ex)
        {
            return ValidationResult<StartJobResult>.Failure(ex.Message);
        }
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
