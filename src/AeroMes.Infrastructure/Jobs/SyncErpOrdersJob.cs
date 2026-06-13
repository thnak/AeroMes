using AeroMes.Application.Integration.Commands.SyncProductionOrders;
using AeroMes.Application.Integration.Commands.SyncSalesOrders;
using AeroMes.Application.Interfaces;
using Hangfire;
using LiteBus.Commands.Abstractions;
using Microsoft.Extensions.Logging;

namespace AeroMes.Infrastructure.Jobs;

public class SyncErpOrdersJob(
    ICommandMediator commandMediator,
    ISystemOptionsRepository optionsRepo,
    ILogger<SyncErpOrdersJob> logger)
{
    [DisableConcurrentExecution(60)]
    public async Task ExecuteAsync(CancellationToken ct = default)
    {
        var options = await optionsRepo.GetAsync(ct);
        if (!options.ErpEnabled)
        {
            logger.LogDebug("ERP sync skipped — ErpEnabled is false.");
            return;
        }

        logger.LogInformation("SyncErpOrdersJob: starting ERP sync.");

        var soResult = await commandMediator.SendAsync(new SyncSalesOrdersCommand(), null, ct);
        if (soResult.IsSuccess)
            logger.LogInformation("SO sync complete — created: {C}, updated: {U}",
                soResult.Value!.Created, soResult.Value.Updated);
        else
            logger.LogWarning("SO sync failed: {Err}", soResult.ErrorMessage);

        var poResult = await commandMediator.SendAsync(new SyncProductionOrdersCommand(), null, ct);
        if (poResult.IsSuccess)
            logger.LogInformation("PO sync complete — created: {C}, updated: {U}",
                poResult.Value!.Created, poResult.Value.Updated);
        else
            logger.LogWarning("PO sync failed: {Err}", poResult.ErrorMessage);
    }
}
