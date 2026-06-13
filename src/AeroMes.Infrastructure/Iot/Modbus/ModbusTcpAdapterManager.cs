using AeroMes.Domain.Iot;
using AeroMes.Domain.Iot.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AeroMes.Infrastructure.Iot.Modbus;

/// <summary>
/// BackgroundService that loads all enabled Modbus TCP adapters on startup
/// and spins up a <see cref="ModbusTcpAdapterService"/> for each one.
/// </summary>
public sealed class ModbusTcpAdapterManager(
    IServiceScopeFactory scopeFactory,
    ISignalIngestionPipeline pipeline,
    ILogger<ModbusTcpAdapterManager> logger,
    ILoggerFactory loggerFactory)
    : BackgroundService
{
    private readonly Dictionary<int, (Task task, CancellationTokenSource cts)> _adapters = [];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("ModbusTcpAdapterManager starting — loading enabled Modbus adapters");

        IReadOnlyList<AdapterInstance> adapters;

        using (var scope = scopeFactory.CreateScope())
        {
            var repo = scope.ServiceProvider.GetRequiredService<IAdapterRepository>();
            adapters = await repo.GetEnabledByTypeAsync(AdapterType.Modbus, stoppingToken);
        }

        logger.LogInformation("ModbusTcpAdapterManager found {Count} enabled Modbus adapter(s)", adapters.Count);

        foreach (var adapter in adapters)
        {
            StartAdapter(adapter, stoppingToken);
        }

        // Wait until the host signals shutdown
        await Task.Delay(Timeout.Infinite, stoppingToken).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);

        logger.LogInformation("ModbusTcpAdapterManager shutting down — stopping {Count} adapter(s)", _adapters.Count);

        // Cancel all adapter services
        foreach (var (_, (_, cts)) in _adapters)
            await cts.CancelAsync();

        // Wait for all adapter tasks to finish
        await Task.WhenAll(_adapters.Values.Select(v => v.task)).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);

        logger.LogInformation("ModbusTcpAdapterManager stopped");
    }

    private void StartAdapter(AdapterInstance adapter, CancellationToken hostToken)
    {
        var cts = CancellationTokenSource.CreateLinkedTokenSource(hostToken);
        var adapterLogger = loggerFactory.CreateLogger<ModbusTcpAdapterService>();
        var service = new ModbusTcpAdapterService(adapter, scopeFactory, pipeline, adapterLogger);

        var task = Task.Run(
            () => service.StartAsync(cts.Token).ContinueWith(
                _ => service.StopAsync(CancellationToken.None),
                TaskContinuationOptions.None),
            cts.Token);

        _adapters[adapter.AdapterID] = (task, cts);

        logger.LogInformation(
            "ModbusTcpAdapterManager: started adapter {Id} for machine '{Machine}'",
            adapter.AdapterID, adapter.MachineCode);
    }
}
