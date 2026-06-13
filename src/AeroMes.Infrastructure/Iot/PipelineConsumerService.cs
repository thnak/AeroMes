using AeroMes.Application.Interfaces;
using AeroMes.Domain.Iot;
using AeroMes.Domain.Iot.Events;
using AeroMes.Infrastructure.Data;
using LiteBus.Events.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AeroMes.Infrastructure.Iot;

public class PipelineConsumerService(
    ISignalIngestionPipeline pipeline,
    IServiceScopeFactory scopeFactory,
    IotPipelineOptions options,
    ILogger<PipelineConsumerService> logger,
    IIotSignalNotifier? hubNotifier = null) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("IoT PipelineConsumerService started.");

        var reader = pipeline.Reader;
        var batch = new List<MachineSignalMessage>(options.BatchSize);
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(options.BatchFlushIntervalMs));

        var timerTask = WaitForTimerAsync(timer, stoppingToken);
        var readTask = reader.WaitToReadAsync(stoppingToken).AsTask();

        while (!stoppingToken.IsCancellationRequested)
        {
            var completed = await Task.WhenAny(timerTask, readTask);

            // Drain available messages
            while (batch.Count < options.BatchSize && reader.TryRead(out var msg))
                batch.Add(msg);

            // Flush if timer fired or batch is full
            bool shouldFlush = completed == timerTask || batch.Count >= options.BatchSize;

            if (shouldFlush && batch.Count > 0)
            {
                await FlushBatchAsync(batch, stoppingToken);
                batch.Clear();
            }

            // Reset tasks
            if (completed == timerTask)
                timerTask = WaitForTimerAsync(timer, stoppingToken);
            else
            {
                if (!stoppingToken.IsCancellationRequested)
                    readTask = reader.WaitToReadAsync(stoppingToken).AsTask();
            }
        }

        // Drain remaining messages on shutdown
        logger.LogInformation("IoT PipelineConsumerService stopping — draining remaining messages.");
        while (reader.TryRead(out var remaining))
        {
            batch.Add(remaining);
            if (batch.Count >= options.BatchSize)
            {
                await FlushBatchAsync(batch, CancellationToken.None);
                batch.Clear();
            }
        }

        if (batch.Count > 0)
            await FlushBatchAsync(batch, CancellationToken.None);

        logger.LogInformation("IoT PipelineConsumerService stopped.");
    }

    private static async Task WaitForTimerAsync(PeriodicTimer timer, CancellationToken ct)
    {
        try { await timer.WaitForNextTickAsync(ct); }
        catch (OperationCanceledException) { /* expected on shutdown */ }
    }

    private async Task FlushBatchAsync(IReadOnlyList<MachineSignalMessage> batch, CancellationToken ct)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var eventMediator = scope.ServiceProvider.GetRequiredService<IEventMediator>();

            foreach (var msg in batch)
            {
                var log = MachineSignalLog.FromMessage(msg);
                db.MachineSignalLogs.Add(log);
            }

            await db.SaveChangesAsync(ct);

            var now = DateTimeOffset.UtcNow;

            foreach (var msg in batch)
            {
                var evt = new MachineSignalIngestedEvent(
                    msg.MachineCode, msg.TagKey, msg.Value, msg.Unit,
                    msg.Timestamp, msg.IsBadQuality);
                await eventMediator.PublishAsync(evt, null, ct);
            }

            if (pipeline is SignalIngestionPipeline concrete)
                concrete.RecordBatch(batch.Count, now);

            // Broadcast live signals via SignalR (best-effort — fire-and-forget per message)
            if (hubNotifier is not null)
            {
                foreach (var msg in batch)
                {
                    _ = hubNotifier.SendSignalAsync(
                        msg.MachineCode, msg.TagKey, msg.Value, msg.Unit,
                        msg.Timestamp, msg.IsBadQuality, CancellationToken.None);
                }
            }

            logger.LogDebug("IoT batch flushed: {Count} messages.", batch.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error flushing IoT signal batch ({Count} messages).", batch.Count);
        }
    }
}
