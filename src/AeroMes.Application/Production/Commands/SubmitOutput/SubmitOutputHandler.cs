using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Production;
using AeroMes.Domain.Production.Repositories;
using AeroMes.Domain.Quality.Repositories;
using MediatR;

namespace AeroMes.Application.Production.Commands.SubmitOutput;

public class SubmitOutputHandler(
    IWorkOrderRepository workOrderRepo,
    IProductionLogRepository productionLogRepo,
    IDefectCodeRepository defectCodeRepo,
    IUnitOfWork uow)
    : IRequestHandler<SubmitOutputCommand, SubmitOutputResult>
{
    public async Task<SubmitOutputResult> Handle(SubmitOutputCommand cmd, CancellationToken ct)
    {
        // Idempotency guard — skip duplicate requests
        if (cmd.IdempotencyKey is not null &&
            await productionLogRepo.ExistsByIdempotencyKeyAsync(cmd.IdempotencyKey, ct))
        {
            var wo = await workOrderRepo.GetByIdAsync(cmd.WorkOrderId, ct);
            return new SubmitOutputResult(
                -1, wo?.ActualQtyOK.Value ?? -1, wo?.ActualQtyNG.Value ?? -1, IsDuplicate: true);
        }

        var workOrder = await workOrderRepo.GetByIdAsync(cmd.WorkOrderId, ct)
            ?? throw new EntityNotFoundException(nameof(WorkOrder), cmd.WorkOrderId);

        workOrder.RecordOutput(cmd.QtyOk, cmd.QtyNg, cmd.OperatorId);

        var log = ProductionLog.Create(
            cmd.WorkOrderId, cmd.QtyOk, cmd.QtyNg, cmd.OperatorId,
            cmd.MachineCode, cmd.ShiftCode, cmd.IdempotencyKey, cmd.Timestamp);

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
