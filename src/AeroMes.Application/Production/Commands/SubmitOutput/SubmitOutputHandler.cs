using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Production;
using AeroMes.Domain.Production.Repositories;
using AeroMes.Domain.Quality.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Application.Production.Commands.SubmitOutput;

public class SubmitOutputHandler(
    IJobRepository jobRepo,
    IWorkOrderRepository workOrderRepo,
    IProductionLogRepository productionLogRepo,
    IDefectCodeRepository defectCodeRepo,
    IUnitOfWork uow,
    IValidator<SubmitOutputCommand> validator)
    : ICommandHandler<SubmitOutputCommand, ValidationResult<SubmitOutputResult>>
{
    public async Task<ValidationResult<SubmitOutputResult>> HandleAsync(SubmitOutputCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<SubmitOutputResult>.Invalid(validation.ToErrorDictionary());

        try
        {
            // Idempotency guard
            if (cmd.IdempotencyKey is not null &&
                await productionLogRepo.ExistsByIdempotencyKeyAsync(cmd.IdempotencyKey, ct))
            {
                return ValidationResult<SubmitOutputResult>.Ok(new SubmitOutputResult(-1, -1, -1, IsDuplicate: true));
            }

            var job = await jobRepo.GetByIdAsync(cmd.JobId, ct);
            if (job is null) return ValidationResult<SubmitOutputResult>.NotFound($"Entity '{cmd.JobId}' was not found.");

            if (job.Status != JobStatus.Active)
                throw new DomainException($"Job {job.JobID} must be Active to submit output. Current: {job.Status}.");

            var workOrder = await workOrderRepo.GetByIdAsync(job.WOID, ct);
            if (workOrder is null) return ValidationResult<SubmitOutputResult>.NotFound($"Entity '{job.WOID}' was not found.");

            workOrder.AccumulateOutput(cmd.QtyOk, cmd.QtyNg, job.OperatorID);

            var log = ProductionLog.Create(
                cmd.JobId, cmd.QtyOk, cmd.QtyNg,
                cmd.DeviceIp, cmd.IdempotencyKey, cmd.Notes, cmd.Timestamp);

            if (cmd.QtyNg > 0 && cmd.Defects.Count > 0)
            {
                var codes = await defectCodeRepo.GetByCodesAsync(
                    cmd.Defects.Select(d => d.DefectCode), ct);

                foreach (var entry in cmd.Defects)
                {
                    if (!codes.TryGetValue(entry.DefectCode, out var defectCode))
                        return ValidationResult<SubmitOutputResult>.NotFound($"DefectCode '{entry.DefectCode}' was not found.");
                    log.AddDefect(defectCode.DefectCodeID, entry.Qty);
                }
            }

            await productionLogRepo.AddAsync(log, ct);
            await uow.SaveChangesAsync(ct);

            return ValidationResult<SubmitOutputResult>.Ok(
                new SubmitOutputResult(log.LogID, workOrder.ActualQtyOK.Value, workOrder.ActualQtyNG.Value));
        }        catch (DomainException ex)
        {
            return ValidationResult<SubmitOutputResult>.Failure(ex.Message);
        }
    }
}
