using AeroMes.Application.Integration.Commands.SyncProductionOrders;
using AeroMes.Application.Integration.Commands.SyncSalesOrders;
using LiteBus.Commands.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AeroMes.Infrastructure.Services;

public class ErpSyncBackgroundService(
    IServiceScopeFactory scopeFactory,
    ILogger<ErpSyncBackgroundService> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("ERP sync background service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            int intervalMinutes = 15;

            try
            {
                using var scope = scopeFactory.CreateScope();
                var optionsRepo = scope.ServiceProvider
                    .GetRequiredService<AeroMes.Application.Interfaces.ISystemOptionsRepository>();
                var mediator = scope.ServiceProvider.GetRequiredService<ICommandMediator>();

                var options = await optionsRepo.GetAsync(stoppingToken);
                intervalMinutes = Math.Max(1, options.ErpSyncIntervalMinutes);

                if (options.ErpEnabled)
                {
                    logger.LogInformation("Running scheduled ERP sync.");

                    var soResult = await mediator.SendAsync(new SyncSalesOrdersCommand(), null, stoppingToken);
                    if (soResult.IsSuccess)
                        logger.LogInformation(
                            "SO sync complete — created: {C}, updated: {U}",
                            soResult.Value!.Created, soResult.Value.Updated);
                    else
                        logger.LogWarning("SO sync failed: {Err}", soResult.ErrorMessage);

                    var poResult = await mediator.SendAsync(new SyncProductionOrdersCommand(), null, stoppingToken);
                    if (poResult.IsSuccess)
                        logger.LogInformation(
                            "PO sync complete — created: {C}, updated: {U}",
                            poResult.Value!.Created, poResult.Value.Updated);
                    else
                        logger.LogWarning("PO sync failed: {Err}", poResult.ErrorMessage);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unhandled error in ERP sync loop.");
            }

            await Task.Delay(TimeSpan.FromMinutes(intervalMinutes), stoppingToken);
        }

        logger.LogInformation("ERP sync background service stopped.");
    }
}
