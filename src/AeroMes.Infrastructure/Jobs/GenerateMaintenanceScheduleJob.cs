using AeroMes.Application.Common;
using AeroMes.Application.Maintenance.Commands.GenerateScheduledMaintenance;
using Hangfire;
using LiteBus.Commands.Abstractions;
using Microsoft.Extensions.Logging;

namespace AeroMes.Infrastructure.Jobs;

public class GenerateMaintenanceScheduleJob(
    ICommandMediator commandMediator,
    ILogger<GenerateMaintenanceScheduleJob> logger)
{
    [DisableConcurrentExecution(120)]
    public async Task ExecuteAsync(CancellationToken ct = default)
    {
        var result = await commandMediator.SendAsync(
            new GenerateScheduledMaintenanceCommand(DateTime.UtcNow), null, ct);

        if (result.IsSuccess)
            logger.LogInformation("GenerateMaintenanceScheduleJob: generated {Count} PM work orders.", result.Value);
        else
            logger.LogWarning("GenerateMaintenanceScheduleJob failed: {Errors}", result.Errors);
    }
}
