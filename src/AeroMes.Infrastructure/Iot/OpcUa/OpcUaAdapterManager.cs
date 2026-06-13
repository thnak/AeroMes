using AeroMes.Domain.Iot;
using AeroMes.Domain.Iot.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AeroMes.Infrastructure.Iot.OpcUa;

/// <summary>
/// BackgroundService that loads all enabled OPC-UA adapters on startup
/// and spins up an <see cref="OpcUaAdapterService"/> for each one.
/// </summary>
public sealed class OpcUaAdapterManager(
    IServiceScopeFactory scopeFactory,
    ISignalIngestionPipeline pipeline,
    ILogger<OpcUaAdapterManager> logger,
    ILoggerFactory loggerFactory)
    : BackgroundService
{
    private readonly Dictionary<int, (Task task, CancellationTokenSource cts)> _adapters = [];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("OpcUaAdapterManager starting — loading enabled OPC-UA adapters");

        IReadOnlyList<AdapterInstance> adapters;

        using (var scope = scopeFactory.CreateScope())
        {
            var repo = scope.ServiceProvider.GetRequiredService<IAdapterRepository>();
            adapters = await repo.GetEnabledByTypeAsync(AdapterType.OpcUa, stoppingToken);
        }

        logger.LogInformation("OpcUaAdapterManager found {Count} enabled OPC-UA adapter(s)", adapters.Count);

        foreach (var adapter in adapters)
        {
            StartAdapter(adapter, stoppingToken);
        }

        // Wait until the host signals shutdown
        await Task.Delay(Timeout.Infinite, stoppingToken).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);

        logger.LogInformation("OpcUaAdapterManager shutting down — stopping {Count} adapter(s)", _adapters.Count);

        // Cancel all adapter services
        foreach (var (_, (_, cts)) in _adapters)
            await cts.CancelAsync();

        // Wait for all adapter tasks to finish
        await Task.WhenAll(_adapters.Values.Select(v => v.task)).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);

        logger.LogInformation("OpcUaAdapterManager stopped");
    }

    private void StartAdapter(AdapterInstance adapter, CancellationToken hostToken)
    {
        var cts = CancellationTokenSource.CreateLinkedTokenSource(hostToken);
        var adapterLogger = loggerFactory.CreateLogger<OpcUaAdapterService>();
        var service = new OpcUaAdapterService(adapter, scopeFactory, pipeline, adapterLogger);

        var task = Task.Run(
            () => service.StartAsync(cts.Token).ContinueWith(
                _ => service.StopAsync(CancellationToken.None),
                TaskContinuationOptions.None),
            cts.Token);

        _adapters[adapter.AdapterID] = (task, cts);

        logger.LogInformation(
            "OpcUaAdapterManager: started adapter {Id} for machine '{Machine}'",
            adapter.AdapterID, adapter.MachineCode);
    }
}
