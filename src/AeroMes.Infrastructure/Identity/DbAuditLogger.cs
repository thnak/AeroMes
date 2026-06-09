using AeroMes.Application.Auth;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Auth;
using AeroMes.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;

namespace AeroMes.Infrastructure.Identity;

public sealed class DbAuditLogger : IHostedService, IAuditLogger
{
    private readonly Channel<SecurityAuditEvent> _channel;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DbAuditLogger> _logger;
    private Task _drainTask = Task.CompletedTask;

    public DbAuditLogger(IServiceScopeFactory scopeFactory, ILogger<DbAuditLogger> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _channel = Channel.CreateBounded<SecurityAuditEvent>(
            new BoundedChannelOptions(2000)
            {
                FullMode = BoundedChannelFullMode.DropOldest,
                SingleReader = true,
            });
    }

    public void Log(SecurityAuditEvent auditEvent)
        => _channel.Writer.TryWrite(auditEvent);

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _drainTask = DrainAsync();
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _channel.Writer.TryComplete();
        try { await _drainTask.WaitAsync(TimeSpan.FromSeconds(10), cancellationToken); }
        catch (Exception ex) when (ex is TimeoutException or OperationCanceledException)
        {
            _logger.LogWarning("Audit log drain timed out on shutdown.");
        }
    }

    private async Task DrainAsync()
    {
        var batch = new List<SecurityAuditLog>(50);

        await foreach (var ev in _channel.Reader.ReadAllAsync())
        {
            batch.Add(MapToEntity(ev));

            while (batch.Count < 50 && _channel.Reader.TryRead(out var more))
                batch.Add(MapToEntity(more));

            await PersistBatchAsync(batch);
            batch.Clear();
        }

        // Flush any remaining items written before TryComplete()
        while (_channel.Reader.TryRead(out var ev))
            batch.Add(MapToEntity(ev));

        if (batch.Count > 0)
            await PersistBatchAsync(batch);
    }

    private async Task PersistBatchAsync(List<SecurityAuditLog> batch)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.SecurityAuditLogs.AddRangeAsync(batch);
            await db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to persist {Count} audit log entries.", batch.Count);
        }
    }

    private static SecurityAuditLog MapToEntity(SecurityAuditEvent ev)
        => SecurityAuditLog.Create(
            ev.EventType, ev.ActorId, ev.ActorType, ev.ActorIp, ev.ActorUserAgent,
            ev.TargetType, ev.TargetId, ev.OldValues, ev.NewValues,
            ev.Outcome, ev.FailureReason);
}
