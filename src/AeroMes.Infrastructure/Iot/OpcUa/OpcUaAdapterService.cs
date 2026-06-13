using System.Text.Json;
using AeroMes.Domain.Iot;
using AeroMes.Domain.Iot.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Configuration;

namespace AeroMes.Infrastructure.Iot.OpcUa;

/// <summary>
/// BackgroundService that manages the OPC-UA connection for a single AdapterInstance.
/// Supports Subscription (monitored items) and Poll modes.
/// Reconnects automatically on disconnect/error.
///
/// NOTE: Certificate-based auth (SignAndEncrypt with client certs) is not implemented here.
/// AutoAcceptUntrustedCertificates = true handles server cert trust for initial deployments.
/// Add proper PKI trust store management when SignAndEncrypt is required.
/// </summary>
public sealed class OpcUaAdapterService : BackgroundService
{
    private readonly AdapterInstance _adapter;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ISignalIngestionPipeline _pipeline;
    private readonly ILogger<OpcUaAdapterService> _logger;

    public OpcUaAdapterService(
        AdapterInstance adapter,
        IServiceScopeFactory scopeFactory,
        ISignalIngestionPipeline pipeline,
        ILogger<OpcUaAdapterService> logger)
    {
        _adapter = adapter;
        _scopeFactory = scopeFactory;
        _pipeline = pipeline;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        OpcUaAdapterConfig config;
        try
        {
            config = JsonSerializer.Deserialize<OpcUaAdapterConfig>(_adapter.ConfigJson)
                     ?? new OpcUaAdapterConfig();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "OPC-UA Adapter {Id} ({Machine}): failed to parse ConfigJson — adapter will not start",
                _adapter.AdapterID, _adapter.MachineCode);
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            Session? session = null;
            try
            {
                session = await CreateSessionAsync(config, stoppingToken);
                if (session is null) break; // cancelled

                _adapter.SetStatus(AdapterStatus.Connected);
                _logger.LogInformation(
                    "OPC-UA Adapter {Id} ({Machine}): connected to {Url}",
                    _adapter.AdapterID, _adapter.MachineCode, config.ServerUrl);

                IReadOnlyList<SignalMapping> mappings;
                using (var scope = _scopeFactory.CreateScope())
                {
                    var repo = scope.ServiceProvider.GetRequiredService<ISignalMappingRepository>();
                    mappings = await repo.GetByAdapterAsync(_adapter.AdapterID, stoppingToken);
                }

                var enabledMappings = mappings.Where(m => m.IsEnabled).ToList();

                if (config.SubscriptionMode == "Poll")
                {
                    await RunPollModeAsync(session, config, enabledMappings, stoppingToken);
                }
                else
                {
                    await RunSubscriptionModeAsync(session, config, enabledMappings, stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                // Normal shutdown
                break;
            }
            catch (Exception ex)
            {
                _adapter.SetStatus(AdapterStatus.Disconnected);
                _logger.LogWarning(ex,
                    "OPC-UA Adapter {Id} ({Machine}): error — reconnecting in {Delay}ms",
                    _adapter.AdapterID, _adapter.MachineCode, config.ReconnectDelayMs);
            }
            finally
            {
                if (session is not null)
                {
                    try
                    {
                        if (session.Connected)
                            await session.CloseAsync();
                        session.Dispose();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex,
                            "OPC-UA Adapter {Id} ({Machine}): error during session cleanup",
                            _adapter.AdapterID, _adapter.MachineCode);
                    }
                }
            }

            if (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(config.ReconnectDelayMs, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }

        _adapter.SetStatus(AdapterStatus.Disconnected);
        _logger.LogInformation(
            "OPC-UA Adapter {Id} ({Machine}): stopped",
            _adapter.AdapterID, _adapter.MachineCode);
    }

    // ── Session creation ──────────────────────────────────────────────────

    private async Task<Session?> CreateSessionAsync(OpcUaAdapterConfig config, CancellationToken ct)
    {
        var appConfig = new ApplicationConfiguration
        {
            ApplicationName = "AeroMes",
            ApplicationUri = "urn:aeromes:client",
            ApplicationType = ApplicationType.Client,
            SecurityConfiguration = new SecurityConfiguration
            {
                ApplicationCertificate = new CertificateIdentifier
                {
                    StoreType = "Directory",
                    StorePath = "./pki/own"
                },
                TrustedPeerCertificates = new CertificateTrustList
                {
                    StoreType = "Directory",
                    StorePath = "./pki/trusted"
                },
                RejectedCertificateStore = new CertificateTrustList
                {
                    StoreType = "Directory",
                    StorePath = "./pki/rejected"
                },
                // AutoAccept for initial deployments. For production with SignAndEncrypt,
                // manage the trusted store explicitly.
                AutoAcceptUntrustedCertificates = true,
                AddAppCertToTrustedStore = true,
            },
            TransportConfigurations = new TransportConfigurationCollection(),
            TransportQuotas = new TransportQuotas { OperationTimeout = 15_000 },
            ClientConfiguration = new ClientConfiguration
            {
                DefaultSessionTimeout = config.SessionTimeoutMs
            },
        };

        await appConfig.Validate(ApplicationType.Client);

        ct.ThrowIfCancellationRequested();

        var useSecurity = !string.Equals(config.SecurityMode, "None", StringComparison.OrdinalIgnoreCase);
        var endpoint = CoreClientUtils.SelectEndpoint(appConfig, config.ServerUrl, useSecurity, 15_000);

        var userIdentity = config.AuthMode switch
        {
            "UsernamePassword" => new UserIdentity(config.Username!, config.Password!),
            _ => new UserIdentity(new AnonymousIdentityToken()),
        };

        ct.ThrowIfCancellationRequested();

        var session = await Session.Create(
            appConfig,
            new ConfiguredEndpoint(null, endpoint, EndpointConfiguration.Create(appConfig)),
            false,
            "AeroMes",
            (uint)config.SessionTimeoutMs,
            userIdentity,
            null);

        return session;
    }

    // ── Subscription mode ─────────────────────────────────────────────────

    private async Task RunSubscriptionModeAsync(
        Session session,
        OpcUaAdapterConfig config,
        List<SignalMapping> mappings,
        CancellationToken stoppingToken)
    {
        if (mappings.Count == 0)
        {
            _logger.LogWarning(
                "OPC-UA Adapter {Id} ({Machine}): no enabled signal mappings — waiting for changes",
                _adapter.AdapterID, _adapter.MachineCode);
            await Task.Delay(Timeout.Infinite, stoppingToken);
            return;
        }

        var subscription = new Subscription(session.DefaultSubscription)
        {
            PublishingInterval = config.PublishingIntervalMs,
            KeepAliveCount = (uint)config.MaxKeepAliveCount,
            PublishingEnabled = true,
        };

        foreach (var mapping in mappings)
        {
            var item = new MonitoredItem(subscription.DefaultItem)
            {
                StartNodeId = NodeId.Parse(mapping.SourceAddress),
                AttributeId = Attributes.Value,
                DisplayName = mapping.TagKey,
                SamplingInterval = config.PublishingIntervalMs,
            };
            item.Notification += (monItem, args) => OnMonitoredItemNotification(monItem, args, stoppingToken);
            subscription.AddItem(item);
        }

        session.AddSubscription(subscription);
        subscription.Create();

        _logger.LogInformation(
            "OPC-UA Adapter {Id} ({Machine}): subscription created with {Count} monitored item(s), interval={Interval}ms",
            _adapter.AdapterID, _adapter.MachineCode, mappings.Count, config.PublishingIntervalMs);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private void OnMonitoredItemNotification(MonitoredItem item, MonitoredItemNotificationEventArgs args, CancellationToken ct)
    {
        if (ct.IsCancellationRequested) return;

        foreach (var value in item.DequeueValues())
        {
            if (!StatusCode.IsGood(value.StatusCode))
            {
                _logger.LogDebug(
                    "OPC-UA Adapter {Id}: bad quality for node '{Node}', status={Status}",
                    _adapter.AdapterID, item.StartNodeId, value.StatusCode);
                continue;
            }

            try
            {
                var raw = Convert.ToDecimal(value.Value);
                var timestamp = value.SourceTimestamp == DateTime.MinValue
                    ? DateTimeOffset.UtcNow
                    : new DateTimeOffset(value.SourceTimestamp, TimeSpan.Zero);

                var message = new AdapterRawMessage(
                    _adapter.AdapterID, item.StartNodeId.ToString()!, raw, timestamp, "OPC-UA");

                // Fire-and-forget into the pipeline (ValueTask, non-blocking)
                _ = _pipeline.IngestAsync(message, ct).AsTask()
                    .ContinueWith(t =>
                    {
                        if (t.IsFaulted)
                            _logger.LogWarning(t.Exception,
                                "OPC-UA Adapter {Id}: pipeline ingest failed for node '{Node}'",
                                _adapter.AdapterID, item.StartNodeId);
                    }, TaskContinuationOptions.OnlyOnFaulted);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "OPC-UA Adapter {Id}: failed to process notification for node '{Node}'",
                    _adapter.AdapterID, item.StartNodeId);
            }
        }
    }

    // ── Poll mode ─────────────────────────────────────────────────────────

    private async Task RunPollModeAsync(
        Session session,
        OpcUaAdapterConfig config,
        List<SignalMapping> mappings,
        CancellationToken stoppingToken)
    {
        if (mappings.Count == 0)
        {
            _logger.LogWarning(
                "OPC-UA Adapter {Id} ({Machine}): no enabled signal mappings — waiting for changes",
                _adapter.AdapterID, _adapter.MachineCode);
            await Task.Delay(Timeout.Infinite, stoppingToken);
            return;
        }

        var nodeIds = mappings
            .Select(m => (NodeId: NodeId.Parse(m.SourceAddress), Mapping: m))
            .ToList();

        _logger.LogInformation(
            "OPC-UA Adapter {Id} ({Machine}): poll mode — {Count} node(s) every {Interval}ms",
            _adapter.AdapterID, _adapter.MachineCode, nodeIds.Count, config.PollIntervalMs);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(config.PollIntervalMs, stoppingToken);

            var nodesToRead = new ReadValueIdCollection(
                nodeIds.Select(n => new ReadValueId
                {
                    NodeId = n.NodeId,
                    AttributeId = Attributes.Value,
                }));

            session.Read(null, 0, TimestampsToReturn.Source, nodesToRead,
                out var results, out _);

            var timestamp = DateTimeOffset.UtcNow;

            for (var i = 0; i < results.Count && i < nodeIds.Count; i++)
            {
                var value = results[i];
                if (!StatusCode.IsGood(value.StatusCode)) continue;

                try
                {
                    var raw = Convert.ToDecimal(value.Value);
                    var ts = value.SourceTimestamp == DateTime.MinValue
                        ? timestamp
                        : new DateTimeOffset(value.SourceTimestamp, TimeSpan.Zero);

                    var message = new AdapterRawMessage(
                        _adapter.AdapterID, nodeIds[i].NodeId.ToString()!, raw, ts, "OPC-UA");

                    await _pipeline.IngestAsync(message, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex,
                        "OPC-UA Adapter {Id}: failed to ingest poll value for node '{Node}'",
                        _adapter.AdapterID, nodeIds[i].NodeId);
                }
            }
        }
    }
}
