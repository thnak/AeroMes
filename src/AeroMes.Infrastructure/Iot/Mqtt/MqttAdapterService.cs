using System.Text;
using System.Text.Json;
using AeroMes.Domain.Iot;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;

namespace AeroMes.Infrastructure.Iot.Mqtt;

/// <summary>
/// BackgroundService that manages the MQTT connection for a single AdapterInstance.
/// Reconnects automatically with exponential back-off on disconnect.
/// </summary>
public sealed class MqttAdapterService : BackgroundService
{
    private readonly AdapterInstance _adapter;
    private readonly ISignalIngestionPipeline _pipeline;
    private readonly ILogger<MqttAdapterService> _logger;

    private IMqttClient? _mqttClient;
    private MqttAdapterConfig _config = new();
    private CancellationToken _stoppingToken;
    private int _reconnectAttempts;

    public MqttAdapterService(
        AdapterInstance adapter,
        ISignalIngestionPipeline pipeline,
        ILogger<MqttAdapterService> logger)
    {
        _adapter = adapter;
        _pipeline = pipeline;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _stoppingToken = stoppingToken;

        try
        {
            _config = JsonSerializer.Deserialize<MqttAdapterConfig>(_adapter.ConfigJson)
                      ?? new MqttAdapterConfig();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Adapter {Id} ({Machine}): failed to parse ConfigJson — adapter will not start",
                _adapter.AdapterID, _adapter.MachineCode);
            return;
        }

        var factory = new MqttFactory();
        _mqttClient = factory.CreateMqttClient();

        _mqttClient.ApplicationMessageReceivedAsync += HandleMessageAsync;
        _mqttClient.DisconnectedAsync += HandleDisconnectedAsync;

        await ConnectAndSubscribeAsync(stoppingToken);

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown
        }

        await CleanupAsync();
    }

    // ── Connect + subscribe ───────────────────────────────────────────────

    private async Task ConnectAndSubscribeAsync(CancellationToken ct)
    {
        if (ct.IsCancellationRequested) return;

        try
        {
            var options = BuildMqttOptions();
            await _mqttClient!.ConnectAsync(options, ct);

            _reconnectAttempts = 0;
            _adapter.SetStatus(AdapterStatus.Connected);
            _logger.LogInformation(
                "Adapter {Id} ({Machine}): connected to MQTT broker {Host}:{Port}",
                _adapter.AdapterID, _adapter.MachineCode, _config.BrokerHost, _config.BrokerPort);

            var topic = $"{_config.TopicPrefix}/{_adapter.MachineCode}/signals/#";
            var qos = (MqttQualityOfServiceLevel)Math.Clamp(_config.QoS, 0, 2);

            var subscribeOptions = new MqttFactory().CreateSubscribeOptionsBuilder()
                .WithTopicFilter(f => f.WithTopic(topic).WithQualityOfServiceLevel(qos))
                .Build();

            await _mqttClient.SubscribeAsync(subscribeOptions, ct);
            _logger.LogInformation(
                "Adapter {Id} ({Machine}): subscribed to '{Topic}' (QoS {QoS})",
                _adapter.AdapterID, _adapter.MachineCode, topic, qos);
        }
        catch (OperationCanceledException)
        {
            // Shutdown in progress — do nothing
        }
        catch (Exception ex)
        {
            _adapter.SetStatus(AdapterStatus.Disconnected);
            _logger.LogWarning(ex,
                "Adapter {Id} ({Machine}): initial connect failed — will retry via DisconnectedAsync",
                _adapter.AdapterID, _adapter.MachineCode);
        }
    }

    private MqttClientOptions BuildMqttOptions()
    {
        var clientId = string.IsNullOrWhiteSpace(_config.ClientId)
            ? $"aeromes-{_adapter.AdapterID}"
            : $"{_config.ClientId}-{_adapter.AdapterID}";

        var builder = new MqttClientOptionsBuilder()
            .WithTcpServer(_config.BrokerHost, _config.BrokerPort)
            .WithClientId(clientId)
            .WithCleanSession(_config.CleanSession)
            .WithKeepAlivePeriod(TimeSpan.FromSeconds(_config.KeepAliveSeconds));

        if (_config.UseTls)
            builder = builder.WithTlsOptions(o => o.UseTls());

        if (!string.IsNullOrEmpty(_config.Username))
            builder = builder.WithCredentials(_config.Username, _config.Password);

        return builder.Build();
    }

    // ── Message handling ──────────────────────────────────────────────────

    private async Task HandleMessageAsync(MqttApplicationMessageReceivedEventArgs e)
    {
        var topic = e.ApplicationMessage.Topic;
        var payloadBytes = e.ApplicationMessage.PayloadSegment;
        var payload = Encoding.UTF8.GetString(payloadBytes);

        // Extract the tag suffix — last segment after the machine code segment
        // Topic pattern: {prefix}/{machineCode}/signals/{tagSuffix...}
        var sourceAddress = ExtractSourceAddress(topic);

        var (value, timestamp) = MqttPayloadParser.Parse(payload, _config, _logger);

        try
        {
            var message = new AdapterRawMessage(
                _adapter.AdapterID, sourceAddress, value, timestamp, "MQTT");

            await _pipeline.IngestAsync(message, _stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Adapter {Id} ({Machine}): failed to ingest message from topic '{Topic}'",
                _adapter.AdapterID, _adapter.MachineCode, topic);
        }
    }

    private string ExtractSourceAddress(string topic)
    {
        // Topic: factory/machines/MACHINE01/signals/temperature
        // prefix: factory/machines  →  prefix segments + 1 (machineCode) + 1 ("signals") = skip
        var prefixDepth = _config.TopicPrefix.Split('/').Length;
        var parts = topic.Split('/');

        // parts[prefixDepth] = machineCode, parts[prefixDepth+1] = "signals", rest = address
        var addrStart = prefixDepth + 2;
        if (addrStart >= parts.Length)
            return topic; // fallback — return whole topic

        return string.Join("/", parts[addrStart..]);
    }

    // ── Disconnect / reconnect ────────────────────────────────────────────

    private async Task HandleDisconnectedAsync(MqttClientDisconnectedEventArgs e)
    {
        if (_stoppingToken.IsCancellationRequested) return;

        _adapter.SetStatus(AdapterStatus.Disconnected);
        _logger.LogWarning(
            "Adapter {Id} ({Machine}): disconnected — reason: {Reason}",
            _adapter.AdapterID, _adapter.MachineCode, e.Reason);

        _reconnectAttempts++;

        if (_config.MaxReconnectAttempts > 0 && _reconnectAttempts > _config.MaxReconnectAttempts)
        {
            _logger.LogError(
                "Adapter {Id} ({Machine}): exceeded max reconnect attempts ({Max}), giving up",
                _adapter.AdapterID, _adapter.MachineCode, _config.MaxReconnectAttempts);
            _adapter.SetStatus(AdapterStatus.Degraded);
            return;
        }

        // Exponential back-off: base * 2^(attempt-1), capped at 60s
        var delayMs = Math.Min(
            _config.ReconnectDelayMs * (int)Math.Pow(2, _reconnectAttempts - 1),
            60_000);

        _logger.LogInformation(
            "Adapter {Id} ({Machine}): reconnecting in {Delay}ms (attempt {Attempt})",
            _adapter.AdapterID, _adapter.MachineCode, delayMs, _reconnectAttempts);

        try
        {
            await Task.Delay(delayMs, _stoppingToken);
            await ConnectAndSubscribeAsync(_stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // Shutdown in progress
        }
    }

    // ── Cleanup ───────────────────────────────────────────────────────────

    private async Task CleanupAsync()
    {
        if (_mqttClient is null) return;

        try
        {
            if (_mqttClient.IsConnected)
            {
                var topic = $"{_config.TopicPrefix}/{_adapter.MachineCode}/signals/#";
                var unsubOptions = new MqttFactory().CreateUnsubscribeOptionsBuilder()
                    .WithTopicFilter(topic)
                    .Build();

                await _mqttClient.UnsubscribeAsync(unsubOptions, CancellationToken.None);
                await _mqttClient.DisconnectAsync(
                    new MqttClientDisconnectOptionsBuilder()
                        .WithReason(MqttClientDisconnectOptionsReason.NormalDisconnection)
                        .Build(),
                    CancellationToken.None);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Adapter {Id} ({Machine}): error during clean disconnect",
                _adapter.AdapterID, _adapter.MachineCode);
        }
        finally
        {
            _mqttClient.Dispose();
        }
    }
}
