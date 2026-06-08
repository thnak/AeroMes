using AeroMes.Application.Interfaces;
using AeroMes.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Application.Production.Commands.SubmitOutput;

public class SubmitOutputHandler(IApplicationDbContext db)
    : IRequestHandler<SubmitOutputCommand, SubmitOutputResult>
{
    public async Task<SubmitOutputResult> Handle(SubmitOutputCommand cmd, CancellationToken ct)
    {
        var workOrder = await db.WorkOrders
            .FirstOrDefaultAsync(x => x.WorkOrderID == cmd.WorkOrderId, ct)
            ?? throw new KeyNotFoundException($"WorkOrder {cmd.WorkOrderId} not found.");

        if (workOrder.Status != WorkOrderStatus.Running)
            throw new InvalidOperationException(
                $"WorkOrder {workOrder.WorkOrderNo} is not in RUNNING state.");

        var log = new ProductionLog
        {
            WorkOrderID = cmd.WorkOrderId,
            Timestamp = cmd.Timestamp,
            QtyOK = cmd.QtyOk,
            QtyNG = cmd.QtyNg,
            OperatorID = cmd.OperatorId,
            MachineCode = cmd.MachineCode,
            ShiftCode = cmd.ShiftCode,
            IdempotencyKey = cmd.IdempotencyKey
        };
        db.ProductionLogs.Add(log);

        if (cmd.QtyNg > 0 && cmd.Defects.Count > 0)
        {
            var defectCodes = await db.DefectCodes
                .Where(x => cmd.Defects.Select(d => d.DefectCode).Contains(x.Code))
                .ToDictionaryAsync(x => x.Code, ct);

            foreach (var entry in cmd.Defects)
            {
                if (!defectCodes.TryGetValue(entry.DefectCode, out var code))
                    throw new KeyNotFoundException($"DefectCode '{entry.DefectCode}' not found.");

                log.DefectDetails.Add(new DefectDetail
                {
                    DefectCodeID = code.DefectCodeID,
                    Quantity = entry.Qty
                });
            }
        }

        // Atomic increment using raw SQL to prevent race conditions (per design doc §5.3)
        await db.WorkOrders
            .Where(x => x.WorkOrderID == cmd.WorkOrderId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.ActualQtyOK, x => x.ActualQtyOK + cmd.QtyOk)
                .SetProperty(x => x.ActualQtyNG, x => x.ActualQtyNG + cmd.QtyNg)
                .SetProperty(x => x.UpdatedAt, DateTime.UtcNow), ct);

        await db.SaveChangesAsync(ct);

        var updated = await db.WorkOrders
            .AsNoTracking()
            .Select(x => new { x.WorkOrderID, x.ActualQtyOK, x.ActualQtyNG })
            .FirstAsync(x => x.WorkOrderID == cmd.WorkOrderId, ct);

        return new SubmitOutputResult(log.LogID, updated.ActualQtyOK, updated.ActualQtyNG);
    }
}
