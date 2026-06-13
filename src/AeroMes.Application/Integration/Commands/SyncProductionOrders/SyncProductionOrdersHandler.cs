using AeroMes.Application.Common;
using AeroMes.Application.Integration.Commands.SyncSalesOrders;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Integration;
using AeroMes.Domain.Integration.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Integration.Commands.SyncProductionOrders;

public class SyncProductionOrdersHandler(
    IErpClient erpClient,
    ISystemOptionsRepository optionsRepo,
    IProductionOrderRepository poRepo,
    ISalesOrderRepository soRepo,
    IUnitOfWork uow)
    : ICommandHandler<SyncProductionOrdersCommand, ValidationResult<ErpSyncResult>>
{
    public async Task<ValidationResult<ErpSyncResult>> HandleAsync(
        SyncProductionOrdersCommand cmd, CancellationToken ct)
    {
        var options = await optionsRepo.GetAsync(ct);
        if (!options.ErpEnabled)
            return ValidationResult<ErpSyncResult>.Failure("ERP integration is disabled.");

        IReadOnlyList<ErpProductionOrderRecord> records;
        try
        {
            records = await erpClient.GetProductionOrdersAsync(options.ErpLastSyncAt, ct);
        }
        catch (Exception ex)
        {
            return ValidationResult<ErpSyncResult>.Failure($"ERP connection error: {ex.Message}");
        }

        int created = 0, updated = 0;
        foreach (var r in records)
        {
            var existing = await poRepo.GetByCodeAsync(r.POCode, ct);
            if (existing is null)
            {
                int? soId = null;
                if (r.SOCode is not null)
                {
                    var so = await soRepo.GetByCodeAsync(r.SOCode, ct);
                    soId = so?.SOID;
                }

                var po = ProductionOrder.CreateFromErp(
                    r.POCode, r.ProductCode, r.TargetQuantity,
                    soId, r.PlannedStart, r.PlannedEnd);
                await poRepo.AddAsync(po, ct);
                created++;
            }
            else
            {
                existing.Resync();
                updated++;
            }
        }

        options.RecordSyncCompleted();
        await uow.SaveChangesAsync(ct);

        return ValidationResult<ErpSyncResult>.Ok(new ErpSyncResult(created, updated, DateTime.UtcNow));
    }
}
