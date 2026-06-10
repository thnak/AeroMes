using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Production;
using AeroMes.Domain.Production.Repositories;
using AeroMes.Domain.Quality.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.Commands.SubmitOutput;

public class SubmitOutputHandler(
    IJobRepository jobRepo,
    IWorkOrderRepository workOrderRepo,
    IProductionLogRepository productionLogRepo,
    IDefectCodeRepository defectCodeRepo,
    IUnitOfWork uow)
    : ICommandHandler<SubmitOutputCommand, SubmitOutputResult>
{
    public async Task<SubmitOutputResult> HandleAsync(SubmitOutputCommand cmd, CancellationToken ct)
    {
        // Idempotency guard
        if (cmd.IdempotencyKey is not null &&
            await productionLogRepo.ExistsByIdempotencyKeyAsync(cmd.IdempotencyKey, ct))
        {
            return new SubmitOutputResult(-1, -1, -1, IsDuplicate: true);
        }

        var job = await jobRepo.GetByIdAsync(cmd.JobId, ct)
            ?? throw new EntityNotFoundException(nameof(Job), cmd.JobId);

        if (job.Status != JobStatus.Active)
            throw new DomainException($"Job {job.JobID} must be Active to submit output. Current: {job.Status}.");

        var workOrder = await workOrderRepo.GetByIdAsync(job.WOID, ct)
            ?? throw new EntityNotFoundException(nameof(WorkOrder), job.WOID);

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
                    throw new EntityNotFoundException("DefectCode", entry.DefectCode);
                log.AddDefect(defectCode.DefectCodeID, entry.Qty);
            }
        }

        await productionLogRepo.AddAsync(log, ct);
        await uow.SaveChangesAsync(ct);

        return new SubmitOutputResult(
            log.LogID, workOrder.ActualQtyOK.Value, workOrder.ActualQtyNG.Value);
    }
}
