using System.Collections.Concurrent;
using System.Threading.Channels;
using AeroMes.Domain.Iot;
using Microsoft.Extensions.Options;

namespace AeroMes.Infrastructure.Iot;

public interface ISignalIngestionPipeline
{
    ValueTask<bool> IngestAsync(AdapterRawMessage message, CancellationToken ct = default);
    PipelineStats GetStats();
    ChannelReader<MachineSignalMessage> Reader { get; }
}

public record PipelineStats(
    long Enqueued, long Dropped, long Processed, int QueueDepth, DateTimeOffset? LastBatchAt);

public class SignalIngestionPipeline : ISignalIngestionPipeline
{
    private readonly ISignalConfigCache _configCache;
    private readonly DeadbandFilter _deadbandFilter;
    private readonly IotPipelineOptions _options;
    private readonly Channel<MachineSignalMessage> _channel;

    private long _enqueued;
    private long _dropped;
    private long _processed;
    private DateTimeOffset? _lastBatchAt;

    public SignalIngestionPipeline(
        ISignalConfigCache configCache,
        DeadbandFilter deadbandFilter,
        IOptions<IotPipelineOptions> options)
    {
        _configCache = configCache;
        _deadbandFilter = deadbandFilter;
        _options = options.Value;

        _channel = Channel.CreateBounded<MachineSignalMessage>(new BoundedChannelOptions(_options.ChannelCapacity)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleReader = true,
            SingleWriter = false
        });
    }

    public ChannelReader<MachineSignalMessage> Reader => _channel.Reader;

    public async ValueTask<bool> IngestAsync(AdapterRawMessage message, CancellationToken ct = default)
    {
        // 1. Resolve SignalMapping from cache
        var mapping = await _configCache.ResolveAsync(message.AdapterId, message.SourceAddress, ct);
        if (mapping is null || !mapping.IsEnabled)
            return false;

        // 2. Scale: value = rawValue * scale + offset
        var value = message.RawValue * (decimal)mapping.Scale + (decimal)mapping.Offset;

        // 3. Bad-quality check
        var isBad = (mapping.QualityMin.HasValue && value < (decimal)mapping.QualityMin.Value)
                 || (mapping.QualityMax.HasValue && value > (decimal)mapping.QualityMax.Value);

        if (isBad && !_options.PersistBadQuality)
        {
            Interlocked.Increment(ref _dropped);
            return false;
        }

        // Get machine code from the adapter navigation property
        var machineCode = mapping.Adapter?.MachineCode ?? string.Empty;

        // 4. Deadband filter
        if (_deadbandFilter.ShouldSkip(
            machineCode, mapping.TagKey, value, message.Timestamp,
            _options.DefaultDeadbandPercent, _options.DefaultMinIntervalMs))
        {
            return false;
        }

        // 5. Write to channel
        var msg = new MachineSignalMessage(
            machineCode, mapping.TagKey, value, null,
            message.Timestamp, message.Source, isBad);

        if (!_channel.Writer.TryWrite(msg))
        {
            Interlocked.Increment(ref _dropped);
            return false;
        }

        Interlocked.Increment(ref _enqueued);
        return true;
    }

    public PipelineStats GetStats() => new(
        Enqueued: Interlocked.Read(ref _enqueued),
        Dropped: Interlocked.Read(ref _dropped),
        Processed: Interlocked.Read(ref _processed),
        QueueDepth: _channel.Reader.Count,
        LastBatchAt: _lastBatchAt);

    internal void RecordBatch(int count, DateTimeOffset at)
    {
        Interlocked.Add(ref _processed, count);
        _lastBatchAt = at;
    }
}
