using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Integration;
using AeroMes.Domain.Integration.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Integration.Commands.SyncSalesOrders;

public class SyncSalesOrdersHandler(
    IErpClient erpClient,
    ISystemOptionsRepository optionsRepo,
    ISalesOrderRepository soRepo,
    IUnitOfWork uow)
    : ICommandHandler<SyncSalesOrdersCommand, ValidationResult<ErpSyncResult>>
{
    public async Task<ValidationResult<ErpSyncResult>> HandleAsync(
        SyncSalesOrdersCommand cmd, CancellationToken ct)
    {
        var options = await optionsRepo.GetAsync(ct);
        if (!options.ErpEnabled)
            return ValidationResult<ErpSyncResult>.Failure("ERP integration is disabled.");

        IReadOnlyList<ErpSalesOrderRecord> records;
        try
        {
            records = await erpClient.GetSalesOrdersAsync(options.ErpLastSyncAt, ct);
        }
        catch (Exception ex)
        {
            return ValidationResult<ErpSyncResult>.Failure($"ERP connection error: {ex.Message}");
        }

        int created = 0, updated = 0;
        foreach (var r in records)
        {
            var existing = await soRepo.GetByCodeAsync(r.SOCode, ct);
            if (existing is null)
            {
                var so = SalesOrder.CreateFromErp(
                    r.SOCode, r.OrderDate, r.CustomerName, r.DeliveryDate, r.CustomerCode);
                await soRepo.AddAsync(so, ct);
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
