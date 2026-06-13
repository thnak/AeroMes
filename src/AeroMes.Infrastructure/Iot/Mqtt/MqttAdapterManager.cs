using AeroMes.Domain.Iot;
using AeroMes.Domain.Iot.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AeroMes.Infrastructure.Iot.Mqtt;

/// <summary>
/// BackgroundService that loads all enabled MQTT adapters on startup
/// and spins up an <see cref="MqttAdapterService"/> for each one.
/// </summary>
public sealed class MqttAdapterManager(
    IServiceScopeFactory scopeFactory,
    ISignalIngestionPipeline pipeline,
    ILogger<MqttAdapterManager> logger,
    ILoggerFactory loggerFactory)
    : BackgroundService
{
    private readonly Dictionary<int, (Task task, CancellationTokenSource cts)> _adapters = [];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("MqttAdapterManager starting — loading enabled MQTT adapters");

        IReadOnlyList<AdapterInstance> adapters;

        using (var scope = scopeFactory.CreateScope())
        {
            var repo = scope.ServiceProvider.GetRequiredService<IAdapterRepository>();
            adapters = await repo.GetEnabledByTypeAsync(AdapterType.Mqtt, stoppingToken);
        }

        logger.LogInformation("MqttAdapterManager found {Count} enabled MQTT adapter(s)", adapters.Count);

        foreach (var adapter in adapters)
        {
            StartAdapter(adapter, stoppingToken);
        }

        // Wait until the host signals shutdown
        await Task.Delay(Timeout.Infinite, stoppingToken).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);

        logger.LogInformation("MqttAdapterManager shutting down — stopping {Count} adapter(s)", _adapters.Count);

        // Cancel all adapter services
        foreach (var (_, (_, cts)) in _adapters)
            await cts.CancelAsync();

        // Wait for all adapter tasks to finish
        await Task.WhenAll(_adapters.Values.Select(v => v.task)).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);

        logger.LogInformation("MqttAdapterManager stopped");
    }

    private void StartAdapter(AdapterInstance adapter, CancellationToken hostToken)
    {
        var cts = CancellationTokenSource.CreateLinkedTokenSource(hostToken);
        var adapterLogger = loggerFactory.CreateLogger<MqttAdapterService>();
        var service = new MqttAdapterService(adapter, pipeline, adapterLogger);

        var task = Task.Run(
            () => service.StartAsync(cts.Token).ContinueWith(
                _ => service.StopAsync(CancellationToken.None),
                TaskContinuationOptions.None),
            cts.Token);

        _adapters[adapter.AdapterID] = (task, cts);

        logger.LogInformation(
            "MqttAdapterManager: started adapter {Id} for machine '{Machine}'",
            adapter.AdapterID, adapter.MachineCode);
    }
}
