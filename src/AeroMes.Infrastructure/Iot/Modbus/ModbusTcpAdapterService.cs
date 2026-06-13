using System.Net;
using System.Text.Json;
using AeroMes.Domain.Iot;
using AeroMes.Domain.Iot.Repositories;
using FluentModbus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AeroMes.Infrastructure.Iot.Modbus;

/// <summary>
/// BackgroundService that manages a Modbus TCP polling loop for a single AdapterInstance.
/// Groups signal mappings into contiguous register blocks and reads them efficiently.
/// Reconnects automatically on error.
/// </summary>
public sealed class ModbusTcpAdapterService : BackgroundService
{
    private readonly AdapterInstance _adapter;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ISignalIngestionPipeline _pipeline;
    private readonly ILogger<ModbusTcpAdapterService> _logger;

    // Semaphore prevents overlapping poll cycles (e.g. slow device + short interval).
    private readonly SemaphoreSlim _pollLock = new(1, 1);

    public ModbusTcpAdapterService(
        AdapterInstance adapter,
        IServiceScopeFactory scopeFactory,
        ISignalIngestionPipeline pipeline,
        ILogger<ModbusTcpAdapterService> logger)
    {
        _adapter = adapter;
        _scopeFactory = scopeFactory;
        _pipeline = pipeline;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        ModbusAdapterConfig config;
        try
        {
            config = JsonSerializer.Deserialize<ModbusAdapterConfig>(_adapter.ConfigJson)
                     ?? new ModbusAdapterConfig();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Modbus Adapter {Id} ({Machine}): failed to parse ConfigJson — adapter will not start",
                _adapter.AdapterID, _adapter.MachineCode);
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            using var client = new ModbusTcpClient();
            client.ConnectTimeout = config.TimeoutMs;
            client.ReadTimeout = config.TimeoutMs;
            client.WriteTimeout = config.TimeoutMs;

            // ── Connect with retry ────────────────────────────────────────────
            var connected = false;
            for (var attempt = 1; attempt <= config.MaxRetries && !stoppingToken.IsCancellationRequested; attempt++)
            {
                try
                {
                    var endpoint = new IPEndPoint(IPAddress.Parse(config.Host), config.Port);
                    client.Connect(endpoint, ModbusEndianness.BigEndian);
                    connected = true;
                    _adapter.SetStatus(AdapterStatus.Connected);
                    _logger.LogInformation(
                        "Modbus Adapter {Id} ({Machine}): connected to {Host}:{Port} (attempt {Attempt})",
                        _adapter.AdapterID, _adapter.MachineCode, config.Host, config.Port, attempt);
                    break;
                }
                catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
                {
                    _adapter.SetStatus(AdapterStatus.Disconnected);
                    _logger.LogWarning(ex,
                        "Modbus Adapter {Id} ({Machine}): connect attempt {Attempt}/{Max} failed",
                        _adapter.AdapterID, _adapter.MachineCode, attempt, config.MaxRetries);

                    if (attempt < config.MaxRetries)
                        await Task.Delay(config.RetryDelayMs, stoppingToken).ConfigureAwait(false);
                }
            }

            if (!connected)
            {
                _logger.LogError(
                    "Modbus Adapter {Id} ({Machine}): all connect attempts failed, backing off 30 s",
                    _adapter.AdapterID, _adapter.MachineCode);
                _adapter.SetStatus(AdapterStatus.Degraded);

                try { await Task.Delay(30_000, stoppingToken); } catch (OperationCanceledException) { break; }
                continue;
            }

            // ── Load signal mappings ──────────────────────────────────────────
            List<(SignalMapping mapping, ModbusAddress address, int regCount)> signals;
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var repo = scope.ServiceProvider.GetRequiredService<ISignalMappingRepository>();
                var mappings = await repo.GetByAdapterAsync(_adapter.AdapterID, stoppingToken);

                signals = [];
                foreach (var m in mappings.Where(m => m.IsEnabled))
                {
                    var addr = ModbusAddressParser.Parse(m.SourceAddress);
                    if (addr is null)
                    {
                        _logger.LogWarning(
                            "Modbus Adapter {Id}: ignoring signal '{Src}' — invalid address format",
                            _adapter.AdapterID, m.SourceAddress);
                        continue;
                    }

                    var regCount = ModbusAddressParser.RegisterCount(addr.DataType, addr.Length);
                    signals.Add((m, addr, regCount));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Modbus Adapter {Id} ({Machine}): failed to load signal mappings",
                    _adapter.AdapterID, _adapter.MachineCode);
                client.Disconnect();
                continue;
            }

            if (signals.Count == 0)
            {
                _logger.LogInformation(
                    "Modbus Adapter {Id} ({Machine}): no enabled signals configured — polling paused 60 s",
                    _adapter.AdapterID, _adapter.MachineCode);
                try { await Task.Delay(60_000, stoppingToken); } catch (OperationCanceledException) { break; }
                client.Disconnect();
                continue;
            }

            // ── Poll loop ─────────────────────────────────────────────────────
            try
            {
                while (!stoppingToken.IsCancellationRequested && client.IsConnected)
                {
                    var pollStart = DateTimeOffset.UtcNow;

                    if (await _pollLock.WaitAsync(0, stoppingToken))
                    {
                        try
                        {
                            await PollOnceAsync(client, config, signals, stoppingToken);
                        }
                        finally
                        {
                            _pollLock.Release();
                        }
                    }
                    else
                    {
                        _logger.LogDebug(
                            "Modbus Adapter {Id}: poll skipped — previous cycle still running",
                            _adapter.AdapterID);
                    }

                    var elapsed = (int)(DateTimeOffset.UtcNow - pollStart).TotalMilliseconds;
                    var remaining = config.PollIntervalMs - elapsed;
                    if (remaining > 0)
                        await Task.Delay(remaining, stoppingToken).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Modbus Adapter {Id} ({Machine}): poll loop error — reconnecting in {Delay} ms",
                    _adapter.AdapterID, _adapter.MachineCode, config.RetryDelayMs);
                _adapter.SetStatus(AdapterStatus.Disconnected);
            }

            if (!client.IsConnected || !stoppingToken.IsCancellationRequested)
            {
                try { client.Disconnect(); } catch { /* ignore */ }
                try { await Task.Delay(config.RetryDelayMs, stoppingToken); } catch (OperationCanceledException) { break; }
            }
        }

        _adapter.SetStatus(AdapterStatus.Disconnected);
        _logger.LogInformation(
            "Modbus Adapter {Id} ({Machine}): stopped",
            _adapter.AdapterID, _adapter.MachineCode);
    }

    // ── Poll one cycle ────────────────────────────────────────────────────────

    private async Task PollOnceAsync(
        ModbusTcpClient client,
        ModbusAdapterConfig config,
        List<(SignalMapping mapping, ModbusAddress address, int regCount)> signals,
        CancellationToken ct)
    {
        // Group by register type
        var byType = signals.GroupBy(s => s.address.RegisterType);

        foreach (var group in byType)
        {
            var regType = group.Key;

            // Sort by address, then merge into contiguous blocks (gap < 5 regs, max 125 regs)
            var sorted = group.OrderBy(s => s.address.Address).ToList();
            var blocks = MergeIntoBlocks(sorted);

            foreach (var block in blocks)
            {
                var blockStart = block[0].address.Address;
                var blockEnd = block[^1].address.Address + block[^1].regCount;
                var blockCount = (ushort)(blockEnd - blockStart);

                Memory<byte> rawBytes;
                try
                {
                    rawBytes = await ReadRegistersAsync(client, config.UnitId, regType, blockStart, blockCount, ct);
                }
                catch (Exception ex) when (!ct.IsCancellationRequested)
                {
                    _logger.LogWarning(ex,
                        "Modbus Adapter {Id}: failed to read {Type} [{Start}..{End}]",
                        _adapter.AdapterID, regType, blockStart, blockEnd);
                    throw; // propagate to reconnect
                }

                // Decode each signal from the block data
                var now = DateTimeOffset.UtcNow;
                foreach (var (mapping, address, regCount) in block)
                {
                    var offsetBytes = (address.Address - blockStart) * 2;
                    var lengthBytes = regCount * 2;

                    if (offsetBytes + lengthBytes > rawBytes.Length)
                    {
                        _logger.LogWarning(
                            "Modbus Adapter {Id}: signal '{Src}' offset {Off} out of range for block of {Len} bytes",
                            _adapter.AdapterID, mapping.SourceAddress, offsetBytes, rawBytes.Length);
                        continue;
                    }

                    var signalBytes = rawBytes.Span.Slice(offsetBytes, lengthBytes);
                    decimal value;

                    if (address.DataType == "STRING")
                    {
                        // For strings, convert the decoded string to 0 (not useful as decimal)
                        // Callers wanting string values need a separate string path — skip for now.
                        continue;
                    }

                    value = ModbusValueDecoder.Decode(signalBytes, address.DataType, config.ByteOrder, config.WordOrder);

                    var msg = new AdapterRawMessage(
                        _adapter.AdapterID, mapping.SourceAddress, value, now, "Modbus");

                    try
                    {
                        await _pipeline.IngestAsync(msg, ct);
                    }
                    catch (Exception ex) when (!ct.IsCancellationRequested)
                    {
                        _logger.LogWarning(ex,
                            "Modbus Adapter {Id}: failed to ingest signal '{Src}'",
                            _adapter.AdapterID, mapping.SourceAddress);
                    }
                }
            }
        }
    }

    // ── Register reading ──────────────────────────────────────────────────────

    private static Task<Memory<byte>> ReadRegistersAsync(
        ModbusTcpClient client,
        byte unitId,
        string regType,
        ushort startAddress,
        ushort count,
        CancellationToken ct)
        => regType switch
        {
            "HR" => client.ReadHoldingRegistersAsync(unitId, startAddress, count, ct),
            "IR" => client.ReadInputRegistersAsync(unitId, startAddress, count, ct),
            "CO" => client.ReadCoilsAsync(unitId, startAddress, count, ct),
            "DI" => client.ReadDiscreteInputsAsync(unitId, startAddress, count, ct),
            _ => throw new ArgumentException($"Unknown register type: {regType}"),
        };

    // ── Block merging ─────────────────────────────────────────────────────────

    private const int MaxBlockSize = 125;   // Modbus spec: max 125 holding/input registers per request
    private const int MergeGapThreshold = 5; // merge if gap between signals is < 5 registers

    private static List<List<(SignalMapping mapping, ModbusAddress address, int regCount)>> MergeIntoBlocks(
        List<(SignalMapping mapping, ModbusAddress address, int regCount)> sorted)
    {
        var blocks = new List<List<(SignalMapping mapping, ModbusAddress address, int regCount)>>();
        if (sorted.Count == 0) return blocks;

        var current = new List<(SignalMapping mapping, ModbusAddress address, int regCount)> { sorted[0] };

        for (var i = 1; i < sorted.Count; i++)
        {
            var prev = current[^1];
            var next = sorted[i];

            var prevEnd = prev.address.Address + prev.regCount;
            var nextStart = next.address.Address;
            var gap = nextStart - prevEnd;

            var currentBlockStart = current[0].address.Address;
            var nextBlockEnd = next.address.Address + next.regCount;
            var wouldBeSize = nextBlockEnd - currentBlockStart;

            if (gap < MergeGapThreshold && wouldBeSize <= MaxBlockSize)
            {
                current.Add(next);
            }
            else
            {
                blocks.Add(current);
                current = [next];
            }
        }

        blocks.Add(current);
        return blocks;
    }
}
