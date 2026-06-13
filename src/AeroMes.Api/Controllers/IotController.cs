using System.Diagnostics;
using System.Text.Json;
using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;
using AeroMes.Application.Iot.Adapters.Commands.CreateAdapter;
using AeroMes.Application.Iot.Adapters.Commands.DeleteAdapter;
using AeroMes.Application.Iot.Adapters.Commands.ToggleAdapter;
using AeroMes.Application.Iot.Adapters.Commands.UpdateAdapter;
using AeroMes.Application.Iot.Adapters.Queries.GetAdapterDetail;
using AeroMes.Application.Iot.Adapters.Queries.GetAdapters;
using AeroMes.Application.Iot.Signals;
using AeroMes.Application.Iot.Signals.Commands.AddSignal;
using AeroMes.Application.Iot.Signals.Commands.CreateSignalTag;
using AeroMes.Application.Iot.Signals.Commands.DeleteSignal;
using AeroMes.Application.Iot.Signals.Commands.DeleteSignalTag;
using AeroMes.Application.Iot.Signals.Commands.ToggleSignal;
using AeroMes.Application.Iot.Signals.Commands.UpdateSignal;
using AeroMes.Application.Iot.Signals.Commands.UpdateSignalTag;
using AeroMes.Application.Iot.Signals.Queries.GetSignalTags;
using AeroMes.Application.Iot.Signals.Queries.GetSignals;
using AeroMes.Application.Iot.StateRules.Commands.CreateStateRule;
using AeroMes.Application.Iot.StateRules.Commands.DeleteStateRule;
using AeroMes.Application.Iot.StateRules.Commands.ReorderStateRules;
using AeroMes.Application.Iot.StateRules.Commands.UpdateStateRule;
using AeroMes.Application.Iot.StateRules.Queries.GetStateRules;
using AeroMes.Domain.Iot;
using AeroMes.Domain.Iot.Repositories;
using AeroMes.Infrastructure.Data;
using AeroMes.Infrastructure.Iot;
using AeroMes.Infrastructure.Iot.Modbus;
using AeroMes.Infrastructure.Iot.Mqtt;
using AeroMes.Infrastructure.Iot.OpcUa;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MQTTnet;
using MQTTnet.Client;
using OpcUa = Opc.Ua;
using OpcUaClient = Opc.Ua.Client;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/iot")]
[Authorize]
public class IotController(
    ICommandMediator commandMediator,
    IQueryMediator queryMediator,
    ISignalIngestionPipeline pipeline,
    IAdapterRepository adapterRepository,
    IAdapterHealthRepository healthRepository,
    AppDbContext db) : ControllerBase
{
    // ── Signal Tags ───────────────────────────────────────────────────────

    [HttpGet("tags")]
    [RequirePermission(Permissions.IotRead)]
    [ProducesResponseType<IReadOnlyList<SignalTagDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSignalTags([FromQuery] string? category, [FromQuery] string? dataType, CancellationToken ct)
        => Ok(await queryMediator.QueryAsync(new GetSignalTagsQuery(category, dataType), null, ct));

    [HttpPost("tags")]
    [RequirePermission(Permissions.IotWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateSignalTag([FromBody] CreateSignalTagRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new CreateSignalTagCommand(req.Key, req.DisplayName, req.Category, req.DataType,
                req.DefaultUnit, req.TypicalMin, req.TypicalMax, req.Description), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpPut("tags/{key}")]
    [RequirePermission(Permissions.IotWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UpdateSignalTag(string key, [FromBody] UpdateSignalTagRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new UpdateSignalTagCommand(key, req.DisplayName, req.Category, req.DataType,
                req.DefaultUnit, req.TypicalMin, req.TypicalMax, req.Description), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpDelete("tags/{key}")]
    [RequirePermission(Permissions.IotWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> DeleteSignalTag(string key, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(new DeleteSignalTagCommand(key), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    // ── Adapters ──────────────────────────────────────────────────────────

    [HttpGet("adapters")]
    [RequirePermission(Permissions.IotRead)]
    [ProducesResponseType<IReadOnlyList<AdapterDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAdapters([FromQuery] string machineCode, CancellationToken ct)
        => Ok(await queryMediator.QueryAsync(new GetAdaptersQuery(machineCode), null, ct));

    [HttpPost("adapters")]
    [RequirePermission(Permissions.IotWrite)]
    [ProducesResponseType<AdapterCreatedResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateAdapter([FromBody] CreateAdapterRequest req, CancellationToken ct)
    {
        if (!Enum.TryParse<AdapterType>(req.AdapterType, ignoreCase: true, out var adapterType))
            return UnprocessableEntity(new { error = $"Invalid AdapterType: {req.AdapterType}" });

        var result = await commandMediator.SendAsync(
            new CreateAdapterCommand(req.MachineCode, adapterType, req.ConfigJson, User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        var id = result.Value!;
        return CreatedAtAction(nameof(GetAdapterDetail), new { id }, new AdapterCreatedResult(id));
    }

    [HttpGet("adapters/{id:int}")]
    [RequirePermission(Permissions.IotRead)]
    [ProducesResponseType<AdapterDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAdapterDetail(int id, CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(new GetAdapterDetailQuery(id), null, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPut("adapters/{id:int}")]
    [RequirePermission(Permissions.IotWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UpdateAdapter(int id, [FromBody] UpdateAdapterRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new UpdateAdapterCommand(id, req.ConfigJson, User.Identity?.Name ?? "system"), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpDelete("adapters/{id:int}")]
    [RequirePermission(Permissions.IotWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAdapter(int id, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new DeleteAdapterCommand(id, User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpPatch("adapters/{id:int}/enable")]
    [RequirePermission(Permissions.IotWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EnableAdapter(int id, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new ToggleAdapterCommand(id, true, User.Identity?.Name ?? "system"), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpPatch("adapters/{id:int}/disable")]
    [RequirePermission(Permissions.IotWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DisableAdapter(int id, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new ToggleAdapterCommand(id, false, User.Identity?.Name ?? "system"), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    // ── Signals ───────────────────────────────────────────────────────────

    [HttpGet("adapters/{id:int}/signals")]
    [RequirePermission(Permissions.IotRead)]
    [ProducesResponseType<IReadOnlyList<SignalDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSignals(int id, CancellationToken ct)
        => Ok(await queryMediator.QueryAsync(new GetSignalsQuery(id), null, ct));

    [HttpPost("adapters/{id:int}/signals")]
    [RequirePermission(Permissions.IotWrite)]
    [ProducesResponseType<SignalCreatedResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> AddSignal(int id, [FromBody] AddSignalRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new AddSignalCommand(id, req.TagKey, req.DisplayName, req.SourceAddress,
                req.Scale, req.Offset, req.QualityMin, req.QualityMax, User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return CreatedAtAction(nameof(GetSignals), new { id }, new SignalCreatedResult(result.Value!));
    }

    [HttpPut("signals/{id:int}")]
    [RequirePermission(Permissions.IotWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UpdateSignal(int id, [FromBody] UpdateSignalRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new UpdateSignalCommand(id, req.DisplayName, req.SourceAddress, req.Scale,
                req.Offset, req.QualityMin, req.QualityMax, User.Identity?.Name ?? "system"), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpDelete("signals/{id:int}")]
    [RequirePermission(Permissions.IotWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSignal(int id, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new DeleteSignalCommand(id, User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpPatch("signals/{id:int}/enable")]
    [RequirePermission(Permissions.IotWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EnableSignal(int id, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new ToggleSignalCommand(id, true, User.Identity?.Name ?? "system"), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpPatch("signals/{id:int}/disable")]
    [RequirePermission(Permissions.IotWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DisableSignal(int id, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new ToggleSignalCommand(id, false, User.Identity?.Name ?? "system"), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    // ── State Rules ───────────────────────────────────────────────────────

    [HttpGet("state-rules")]
    [RequirePermission(Permissions.IotRead)]
    [ProducesResponseType<IReadOnlyList<StateRuleDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStateRules([FromQuery] string machineCode, CancellationToken ct)
        => Ok(await queryMediator.QueryAsync(new GetStateRulesQuery(machineCode), null, ct));

    [HttpPost("state-rules")]
    [RequirePermission(Permissions.IotWrite)]
    [ProducesResponseType<StateRuleCreatedResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateStateRule([FromBody] CreateStateRuleRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new CreateStateRuleCommand(req.MachineCode, req.Priority, req.TargetState, req.SignalTagKey,
                req.Operator, req.ThresholdValue, req.Hysteresis, req.MinDurationMs,
                req.Description, User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Created($"api/v1/iot/state-rules/{result.Value!}", new StateRuleCreatedResult(result.Value!));
    }

    [HttpPut("state-rules/{id:int}")]
    [RequirePermission(Permissions.IotWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UpdateStateRule(int id, [FromBody] UpdateStateRuleRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new UpdateStateRuleCommand(id, req.TargetState, req.SignalTagKey, req.Operator,
                req.ThresholdValue, req.Hysteresis, req.MinDurationMs, req.Description,
                req.IsActive, User.Identity?.Name ?? "system"), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpDelete("state-rules/{id:int}")]
    [RequirePermission(Permissions.IotWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteStateRule(int id, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new DeleteStateRuleCommand(id, User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpPut("state-rules/reorder")]
    [RequirePermission(Permissions.IotWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> ReorderStateRules([FromBody] ReorderStateRulesRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new ReorderStateRulesCommand(req.MachineCode, req.OrderedRuleIds, User.Identity?.Name ?? "system"), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    // ── Machine State ─────────────────────────────────────────────────────

    [HttpGet("machines/states")]
    [RequirePermission(Permissions.IotRead)]
    [ProducesResponseType<IReadOnlyList<MachineStateSnapshotDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMachineStates(CancellationToken ct)
    {
        var snapshots = await db.MachineStateSnapshots
            .AsNoTracking()
            .ToListAsync(ct);

        var result = snapshots.Select(s => new MachineStateSnapshotDto(
            s.MachineCode, s.CurrentState, s.PreviousState, s.StateChangedAt,
            s.TriggerTagKey, s.TriggerValue, s.SignalStaleSince.HasValue)).ToList();

        return Ok((IReadOnlyList<MachineStateSnapshotDto>)result);
    }

    [HttpGet("machines/{code}/state")]
    [RequirePermission(Permissions.IotRead)]
    [ProducesResponseType<MachineStateSnapshotDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMachineState(string code, CancellationToken ct)
    {
        var s = await db.MachineStateSnapshots
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.MachineCode == code, ct);

        if (s is null) return NotFound();

        return Ok(new MachineStateSnapshotDto(
            s.MachineCode, s.CurrentState, s.PreviousState, s.StateChangedAt,
            s.TriggerTagKey, s.TriggerValue, s.SignalStaleSince.HasValue));
    }

    [HttpGet("machines/{code}/state/history")]
    [RequirePermission(Permissions.IotRead)]
    [ProducesResponseType<IReadOnlyList<MachineStateHistoryDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMachineStateHistory(
        string code,
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        [FromQuery] int limit = 100,
        CancellationToken ct = default)
    {
        var query = db.MachineStateHistories
            .AsNoTracking()
            .Where(h => h.MachineCode == code);

        if (from.HasValue) query = query.Where(h => h.TransitionAt >= from.Value);
        if (to.HasValue) query = query.Where(h => h.TransitionAt <= to.Value);

        var rows = await query
            .OrderByDescending(h => h.TransitionAt)
            .Take(Math.Clamp(limit, 1, 1000))
            .ToListAsync(ct);

        var result = rows.Select(h => new MachineStateHistoryDto(
            h.HistoryId, h.MachineCode, h.FromState, h.ToState,
            h.TransitionAt, h.DurationMs, h.TriggerTagKey, h.IsAutomatic)).ToList();

        return Ok((IReadOnlyList<MachineStateHistoryDto>)result);
    }

    [HttpPatch("machines/{code}/state/override")]
    [RequirePermission(Permissions.IotWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> OverrideMachineState(
        string code, [FromBody] StateOverrideRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.TargetState))
            return UnprocessableEntity(new { error = "TargetState is required." });

        var snapshot = await db.MachineStateSnapshots.FindAsync([code], ct);
        if (snapshot is null) return NotFound();

        var previousState = snapshot.CurrentState;
        var stateChangedAt = snapshot.StateChangedAt;

        if (snapshot.TransitionTo(req.TargetState, null, null, null))
        {
            db.MachineStateHistories.Add(MachineStateHistory.Record(
                code, previousState, req.TargetState,
                stateChangedAt, null, null, null, isAutomatic: false));

            await db.SaveChangesAsync(ct);
        }

        return NoContent();
    }

    // ── MQTT Test Connection ──────────────────────────────────────────────

    [HttpPost("adapters/{id:int}/test-connection")]
    [RequirePermission(Permissions.IotWrite)]
    [ProducesResponseType<MqttConnectionTestResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> TestMqttConnection(int id, CancellationToken ct)
    {
        var adapter = await adapterRepository.GetByIdAsync(id, ct);
        if (adapter is null) return NotFound();

        if (adapter.AdapterType != AdapterType.Mqtt)
            return UnprocessableEntity(new { error = "Adapter is not of type MQTT" });

        MqttAdapterConfig config;
        try
        {
            config = JsonSerializer.Deserialize<MqttAdapterConfig>(adapter.ConfigJson)
                     ?? new MqttAdapterConfig();
        }
        catch (Exception ex)
        {
            return UnprocessableEntity(new { error = $"Invalid ConfigJson: {ex.Message}" });
        }

        var factory = new MqttFactory();
        using var mqttClient = factory.CreateMqttClient();

        var clientId = $"aeromes-test-{id}-{Guid.NewGuid():N}";
        var optionsBuilder = new MqttClientOptionsBuilder()
            .WithTcpServer(config.BrokerHost, config.BrokerPort)
            .WithClientId(clientId)
            .WithCleanSession(true)
            .WithKeepAlivePeriod(TimeSpan.FromSeconds(config.KeepAliveSeconds));

        if (config.UseTls)
            optionsBuilder = optionsBuilder.WithTlsOptions(o => o.UseTls());

        if (!string.IsNullOrEmpty(config.Username))
            optionsBuilder = optionsBuilder.WithCredentials(config.Username, config.Password);

        var options = optionsBuilder.Build();

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(5));

        var sw = Stopwatch.StartNew();
        try
        {
            await mqttClient.ConnectAsync(options, timeoutCts.Token);
            sw.Stop();

            await mqttClient.DisconnectAsync(
                new MqttClientDisconnectOptionsBuilder()
                    .WithReason(MqttClientDisconnectOptionsReason.NormalDisconnection)
                    .Build(),
                CancellationToken.None);

            return Ok(new MqttConnectionTestResult(
                Success: true,
                LatencyMs: (int)sw.ElapsedMilliseconds,
                BrokerHost: config.BrokerHost,
                BrokerPort: config.BrokerPort,
                Error: null));
        }
        catch (OperationCanceledException)
        {
            return UnprocessableEntity(new MqttConnectionTestResult(
                Success: false,
                LatencyMs: 5000,
                BrokerHost: config.BrokerHost,
                BrokerPort: config.BrokerPort,
                Error: "Connection timed out after 5 seconds"));
        }
        catch (Exception ex)
        {
            return UnprocessableEntity(new MqttConnectionTestResult(
                Success: false,
                LatencyMs: (int)sw.ElapsedMilliseconds,
                BrokerHost: config.BrokerHost,
                BrokerPort: config.BrokerPort,
                Error: ex.Message));
        }
    }

    // ── OPC-UA commissioning ──────────────────────────────────────────────

    [HttpPost("adapters/{id:int}/browse")]
    [RequirePermission(Permissions.IotWrite)]
    [ProducesResponseType<IReadOnlyList<OpcNodeInfo>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> BrowseOpcUaNode(int id, [FromBody] OpcBrowseRequest req, CancellationToken ct)
    {
        var adapter = await adapterRepository.GetByIdAsync(id, ct);
        if (adapter is null) return NotFound();

        if (adapter.AdapterType != AdapterType.OpcUa)
            return UnprocessableEntity(new { error = "Adapter is not of type OPC-UA" });

        OpcUaAdapterConfig config;
        try
        {
            config = JsonSerializer.Deserialize<OpcUaAdapterConfig>(adapter.ConfigJson)
                     ?? new OpcUaAdapterConfig();
        }
        catch (Exception ex)
        {
            return UnprocessableEntity(new { error = $"Invalid ConfigJson: {ex.Message}" });
        }

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(10));

        OpcUaClient.Session? session = null;
        try
        {
            var appConfig = BuildOpcAppConfig();
            await appConfig.Validate(OpcUa.ApplicationType.Client);

            var useSecurity = !string.Equals(config.SecurityMode, "None", StringComparison.OrdinalIgnoreCase);
            var endpoint = OpcUaClient.CoreClientUtils.SelectEndpoint(appConfig, config.ServerUrl, useSecurity, 15_000);

            var userIdentity = config.AuthMode == "UsernamePassword"
                ? new OpcUa.UserIdentity(config.Username!, config.Password!)
                : new OpcUa.UserIdentity(new OpcUa.AnonymousIdentityToken());

            session = await OpcUaClient.Session.Create(
                appConfig,
                new OpcUa.ConfiguredEndpoint(null, endpoint, OpcUa.EndpointConfiguration.Create(appConfig)),
                false, "AeroMes-Browse", (uint)config.SessionTimeoutMs, userIdentity, null);

            var startNode = string.IsNullOrWhiteSpace(req.NodeId)
                ? OpcUa.ObjectIds.ObjectsFolder
                : OpcUa.NodeId.Parse(req.NodeId);

            var browser = new OpcUaClient.Browser(session)
            {
                BrowseDirection = OpcUa.BrowseDirection.Forward,
                ReferenceTypeId = OpcUa.ReferenceTypeIds.HierarchicalReferences,
                IncludeSubtypes = true,
                NodeClassMask = 0,
                ResultMask = (uint)OpcUa.BrowseResultMask.All,
            };

            var references = browser.Browse(startNode);

            var nodes = references.Select(r => new OpcNodeInfo(
                NodeId: r.NodeId.ToString(),
                DisplayName: r.DisplayName.Text,
                NodeClass: r.NodeClass.ToString(),
                DataType: string.Empty // DataType requires additional read — omit for browse
            )).ToList();

            return Ok((IReadOnlyList<OpcNodeInfo>)nodes);
        }
        catch (OperationCanceledException)
        {
            return UnprocessableEntity(new { error = "OPC-UA browse timed out" });
        }
        catch (Exception ex)
        {
            return UnprocessableEntity(new { error = ex.Message });
        }
        finally
        {
            if (session is not null)
            {
                try { if (session.Connected) await session.CloseAsync(); } catch { /* ignore */ }
                session.Dispose();
            }
        }
    }

    [HttpPost("adapters/{id:int}/read-node")]
    [RequirePermission(Permissions.IotWrite)]
    [ProducesResponseType<OpcNodeValue>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> ReadOpcUaNode(int id, [FromBody] OpcReadNodeRequest req, CancellationToken ct)
    {
        var adapter = await adapterRepository.GetByIdAsync(id, ct);
        if (adapter is null) return NotFound();

        if (adapter.AdapterType != AdapterType.OpcUa)
            return UnprocessableEntity(new { error = "Adapter is not of type OPC-UA" });

        OpcUaAdapterConfig config;
        try
        {
            config = JsonSerializer.Deserialize<OpcUaAdapterConfig>(adapter.ConfigJson)
                     ?? new OpcUaAdapterConfig();
        }
        catch (Exception ex)
        {
            return UnprocessableEntity(new { error = $"Invalid ConfigJson: {ex.Message}" });
        }

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(10));

        OpcUaClient.Session? session = null;
        try
        {
            var appConfig = BuildOpcAppConfig();
            await appConfig.Validate(OpcUa.ApplicationType.Client);

            var useSecurity = !string.Equals(config.SecurityMode, "None", StringComparison.OrdinalIgnoreCase);
            var endpoint = OpcUaClient.CoreClientUtils.SelectEndpoint(appConfig, config.ServerUrl, useSecurity, 15_000);

            var userIdentity = config.AuthMode == "UsernamePassword"
                ? new OpcUa.UserIdentity(config.Username!, config.Password!)
                : new OpcUa.UserIdentity(new OpcUa.AnonymousIdentityToken());

            session = await OpcUaClient.Session.Create(
                appConfig,
                new OpcUa.ConfiguredEndpoint(null, endpoint, OpcUa.EndpointConfiguration.Create(appConfig)),
                false, "AeroMes-Read", (uint)config.SessionTimeoutMs, userIdentity, null);

            var nodeId = OpcUa.NodeId.Parse(req.NodeId);

            var nodesToRead = new OpcUa.ReadValueIdCollection
            {
                new OpcUa.ReadValueId { NodeId = nodeId, AttributeId = OpcUa.Attributes.Value },
                new OpcUa.ReadValueId { NodeId = nodeId, AttributeId = OpcUa.Attributes.DataType },
            };

            session.Read(null, 0, OpcUa.TimestampsToReturn.Source, nodesToRead,
                out var results, out _);

            var valueResult = results[0];
            var dataTypeResult = results[1];

            var dataTypeName = string.Empty;
            if (OpcUa.StatusCode.IsGood(dataTypeResult.StatusCode) && dataTypeResult.Value is OpcUa.NodeId dtNodeId)
            {
                dataTypeName = dtNodeId.ToString();
            }

            var sourceTs = valueResult.SourceTimestamp == DateTime.MinValue
                ? DateTimeOffset.UtcNow
                : new DateTimeOffset(valueResult.SourceTimestamp, TimeSpan.Zero);

            var nodeValue = new OpcNodeValue(
                NodeId: req.NodeId,
                Value: valueResult.Value?.ToString() ?? string.Empty,
                DataType: dataTypeName,
                StatusCode: valueResult.StatusCode.ToString(),
                SourceTimestamp: sourceTs);

            return Ok(nodeValue);
        }
        catch (OperationCanceledException)
        {
            return UnprocessableEntity(new { error = "OPC-UA read timed out" });
        }
        catch (Exception ex)
        {
            return UnprocessableEntity(new { error = ex.Message });
        }
        finally
        {
            if (session is not null)
            {
                try { if (session.Connected) await session.CloseAsync(); } catch { /* ignore */ }
                session.Dispose();
            }
        }
    }

    private static OpcUa.ApplicationConfiguration BuildOpcAppConfig() => new()
    {
        ApplicationName = "AeroMes",
        ApplicationUri = "urn:aeromes:client",
        ApplicationType = OpcUa.ApplicationType.Client,
        SecurityConfiguration = new OpcUa.SecurityConfiguration
        {
            ApplicationCertificate = new OpcUa.CertificateIdentifier
            {
                StoreType = "Directory",
                StorePath = "./pki/own"
            },
            TrustedPeerCertificates = new OpcUa.CertificateTrustList
            {
                StoreType = "Directory",
                StorePath = "./pki/trusted"
            },
            RejectedCertificateStore = new OpcUa.CertificateTrustList
            {
                StoreType = "Directory",
                StorePath = "./pki/rejected"
            },
            AutoAcceptUntrustedCertificates = true,
            AddAppCertToTrustedStore = true,
        },
        TransportConfigurations = new OpcUa.TransportConfigurationCollection(),
        TransportQuotas = new OpcUa.TransportQuotas { OperationTimeout = 15_000 },
        ClientConfiguration = new OpcUa.ClientConfiguration { DefaultSessionTimeout = 30_000 },
    };

    // ── Modbus commissioning ──────────────────────────────────────────────

    [HttpPost("adapters/{id:int}/read-registers")]
    [RequirePermission(Permissions.IotWrite)]
    [ProducesResponseType<ModbusReadRegistersResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> ReadModbusRegisters(
        int id,
        [FromBody] ModbusReadRegistersRequest req,
        CancellationToken ct)
    {
        var adapter = await adapterRepository.GetByIdAsync(id, ct);
        if (adapter is null) return NotFound();

        if (adapter.AdapterType != AdapterType.Modbus)
            return UnprocessableEntity(new { error = "Adapter is not of type Modbus" });

        ModbusAdapterConfig config;
        try
        {
            config = JsonSerializer.Deserialize<ModbusAdapterConfig>(adapter.ConfigJson)
                     ?? new ModbusAdapterConfig();
        }
        catch (Exception ex)
        {
            return UnprocessableEntity(new { error = $"Invalid ConfigJson: {ex.Message}" });
        }

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeoutCts.CancelAfter(TimeSpan.FromMilliseconds(config.TimeoutMs));

        using var client = new FluentModbus.ModbusTcpClient
        {
            ConnectTimeout = config.TimeoutMs,
            ReadTimeout = config.TimeoutMs,
            WriteTimeout = config.TimeoutMs,
        };

        var sw = Stopwatch.StartNew();
        try
        {
            var endpoint = new System.Net.IPEndPoint(
                System.Net.IPAddress.Parse(config.Host), config.Port);
            client.Connect(endpoint, FluentModbus.ModbusEndianness.BigEndian);
            sw.Stop();

            var regType = req.RegisterType.ToUpperInvariant();
            var quantity = (ushort)Math.Min(req.Count, 125);

            Memory<byte> rawBytes = regType switch
            {
                "HR" => await client.ReadHoldingRegistersAsync(config.UnitId, req.StartAddress, quantity, timeoutCts.Token),
                "IR" => await client.ReadInputRegistersAsync(config.UnitId, req.StartAddress, quantity, timeoutCts.Token),
                "CO" => await client.ReadCoilsAsync(config.UnitId, req.StartAddress, quantity, timeoutCts.Token),
                "DI" => await client.ReadDiscreteInputsAsync(config.UnitId, req.StartAddress, quantity, timeoutCts.Token),
                _ => throw new ArgumentException($"Unknown register type: {req.RegisterType}. Use HR, IR, CO, or DI."),
            };

            var registers = new List<ModbusRegisterValue>();
            var bytesPerRegister = (regType is "CO" or "DI") ? 1 : 2;

            for (var i = 0; i < quantity; i++)
            {
                var byteOffset = i * bytesPerRegister;
                if (byteOffset >= rawBytes.Length) break;

                var sliceLen = Math.Min(bytesPerRegister, rawBytes.Length - byteOffset);
                var slice = rawBytes.Span.Slice(byteOffset, sliceLen);

                var rawHex = Convert.ToHexString(slice);
                float parsedFloat = 0f;

                if (sliceLen >= 2)
                {
                    // Read as big-endian FLOAT32 for convenience (takes 2 regs = 4 bytes)
                    if (sliceLen >= 4)
                        parsedFloat = BitConverter.Int32BitsToSingle(
                            System.Buffers.Binary.BinaryPrimitives.ReadInt32BigEndian(slice));
                    else
                        parsedFloat = System.Buffers.Binary.BinaryPrimitives.ReadUInt16BigEndian(slice);
                }
                else if (sliceLen == 1)
                {
                    parsedFloat = slice[0];
                }

                registers.Add(new ModbusRegisterValue(
                    Address: (ushort)(req.StartAddress + i),
                    RawHex: rawHex,
                    ParsedFloat: parsedFloat));
            }

            client.Disconnect();
            return Ok(new ModbusReadRegistersResponse(
                Host: config.Host,
                Port: config.Port,
                RegisterType: req.RegisterType,
                StartAddress: req.StartAddress,
                LatencyMs: (int)sw.ElapsedMilliseconds,
                Registers: registers));
        }
        catch (OperationCanceledException)
        {
            return UnprocessableEntity(new { error = $"Modbus read timed out after {config.TimeoutMs} ms" });
        }
        catch (ArgumentException ex)
        {
            return UnprocessableEntity(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return UnprocessableEntity(new { error = ex.Message });
        }
    }

    // ── Pipeline ──────────────────────────────────────────────────────────

    [HttpGet("pipeline/stats")]
    [RequirePermission(Permissions.IotRead)]
    [ProducesResponseType<PipelineStats>(StatusCodes.Status200OK)]
    public IActionResult GetPipelineStats()
        => Ok(pipeline.GetStats());

    [HttpPost("pipeline/flush")]
    [RequirePermission(Permissions.IotWrite)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult FlushPipeline()
        => Ok(new { message = "Flush acknowledged. Consumer flushes automatically on interval." });

    // ── Webhook Ingest ────────────────────────────────────────────────────

    private static readonly TimeSpan TimestampTolerance = TimeSpan.FromMinutes(5);
    private const int MaxSignalsPerRequest = 500;

    private async Task<AdapterInstance?> ResolveWebhookAdapter(HttpRequest request, CancellationToken ct)
    {
        if (!request.Headers.TryGetValue("X-Api-Key", out var keyValues) || keyValues.Count == 0)
            return null;
        var apiKey = keyValues[0];
        if (string.IsNullOrWhiteSpace(apiKey))
            return null;
        var adapter = await adapterRepository.GetByApiKeyAsync(apiKey!, ct);
        if (adapter is null || adapter.AdapterType != AdapterType.Webhook)
            return null;
        return adapter;
    }

    private static (List<WebhookSignalPayload> signals, string? error) ParseSignals(JsonDocument doc)
    {
        var signals = new List<WebhookSignalPayload>();

        if (doc.RootElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var el in doc.RootElement.EnumerateArray())
            {
                var payload = ParseSingleSignal(el);
                if (payload is not null)
                    signals.Add(payload);
            }
        }
        else if (doc.RootElement.ValueKind == JsonValueKind.Object &&
                 doc.RootElement.TryGetProperty("signals", out _))
        {
            // Machine bundle: { machineCode, timestamp?, signals: { tagKey: value } }
            var machineCode = doc.RootElement.TryGetProperty("machineCode", out var mc) ? mc.GetString() : null;
            DateTimeOffset? bundleTs = null;
            if (doc.RootElement.TryGetProperty("timestamp", out var tsProp))
            {
                if (tsProp.TryGetDateTimeOffset(out var ts))
                    bundleTs = ts;
            }
            if (doc.RootElement.TryGetProperty("signals", out var signalsProp))
            {
                foreach (var prop in signalsProp.EnumerateObject())
                {
                    var value = prop.Value.ValueKind switch
                    {
                        JsonValueKind.Number => prop.Value.GetDecimal(),
                        _ => 0m
                    };
                    signals.Add(new WebhookSignalPayload(machineCode ?? string.Empty, prop.Name, null, value, null, bundleTs));
                }
            }
        }
        else if (doc.RootElement.ValueKind == JsonValueKind.Object)
        {
            var payload = ParseSingleSignal(doc.RootElement);
            if (payload is not null)
                signals.Add(payload);
        }
        else
        {
            return (signals, "Unrecognized payload shape");
        }

        return (signals, null);
    }

    private static WebhookSignalPayload? ParseSingleSignal(JsonElement el)
    {
        var machineCode = el.TryGetProperty("machineCode", out var mc) ? mc.GetString() : null;
        var tagKey = el.TryGetProperty("tagKey", out var tk) ? tk.GetString() : null;
        var sourceAddress = el.TryGetProperty("sourceAddress", out var sa) ? sa.GetString() : null;
        decimal value = 0;
        if (el.TryGetProperty("value", out var vp) && vp.ValueKind == JsonValueKind.Number)
            value = vp.GetDecimal();
        var unit = el.TryGetProperty("unit", out var u) ? u.GetString() : null;
        DateTimeOffset? timestamp = null;
        if (el.TryGetProperty("timestamp", out var tsProp) && tsProp.TryGetDateTimeOffset(out var ts))
            timestamp = ts;
        return new WebhookSignalPayload(machineCode ?? string.Empty, tagKey, sourceAddress, value, unit, timestamp);
    }

    private async Task<(WebhookIngestResponse response, int statusCode)> ProcessSignalsAsync(
        List<WebhookSignalPayload> signals, int adapterId, bool persist, CancellationToken ct)
    {
        var errors = new List<WebhookIngestError>();
        var accepted = 0;
        var rejected = 0;
        var now = DateTimeOffset.UtcNow;

        foreach (var (signal, idx) in signals.Select((s, i) => (s, i)))
        {
            if (signal.Timestamp.HasValue)
            {
                var diff = (signal.Timestamp.Value - now).Duration();
                if (diff > TimestampTolerance)
                {
                    errors.Add(new WebhookIngestError(idx, signal.TagKey, "Timestamp out of tolerance"));
                    rejected++;
                    continue;
                }
            }

            bool ok;
            if (persist)
            {
                var msg = new AdapterRawMessage(
                    adapterId,
                    signal.SourceAddress ?? signal.TagKey ?? string.Empty,
                    signal.Value,
                    signal.Timestamp ?? now,
                    "Webhook");
                ok = await pipeline.IngestAsync(msg, ct);
            }
            else
            {
                ok = true; // test mode: treat all timestamp-valid signals as accepted
            }

            if (ok)
                accepted++;
            else
            {
                errors.Add(new WebhookIngestError(idx, signal.TagKey, "Signal rejected by pipeline (unmapped or filtered)"));
                rejected++;
            }
        }

        var statusCode = accepted == 0 && rejected > 0 ? 400
            : rejected > 0 ? 207
            : 202;

        return (new WebhookIngestResponse(accepted, rejected, errors.Count > 0 ? errors : null), statusCode);
    }

    [HttpPost("ingest")]
    [AllowAnonymous]
    [ProducesResponseType<WebhookIngestResponse>(StatusCodes.Status202Accepted)]
    [ProducesResponseType<WebhookIngestResponse>(StatusCodes.Status207MultiStatus)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> WebhookIngest(CancellationToken ct)
    {
        var adapter = await ResolveWebhookAdapter(Request, ct);
        if (adapter is null)
            return Unauthorized();

        JsonDocument doc;
        try
        {
            doc = await JsonDocument.ParseAsync(Request.Body, cancellationToken: ct);
        }
        catch
        {
            return BadRequest(new { error = "Invalid JSON body" });
        }

        using (doc)
        {
            var (signals, parseError) = ParseSignals(doc);
            if (parseError is not null)
                return BadRequest(new { error = parseError });

            if (signals.Count > MaxSignalsPerRequest)
                return BadRequest(new { error = $"Batch limit exceeded: max {MaxSignalsPerRequest} signals per request." });

            var (response, statusCode) = await ProcessSignalsAsync(signals, adapter.AdapterID, persist: true, ct);
            return statusCode switch
            {
                400 => BadRequest(response),
                207 => StatusCode(StatusCodes.Status207MultiStatus, response),
                _ => Accepted(response),
            };
        }
    }

    [HttpPost("ingest/test")]
    [AllowAnonymous]
    [ProducesResponseType<WebhookIngestResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> WebhookIngestTest(CancellationToken ct)
    {
        var adapter = await ResolveWebhookAdapter(Request, ct);
        if (adapter is null)
            return Unauthorized();

        JsonDocument doc;
        try
        {
            doc = await JsonDocument.ParseAsync(Request.Body, cancellationToken: ct);
        }
        catch
        {
            return BadRequest(new { error = "Invalid JSON body" });
        }

        using (doc)
        {
            var (signals, parseError) = ParseSignals(doc);
            if (parseError is not null)
                return BadRequest(new { error = parseError });

            if (signals.Count > MaxSignalsPerRequest)
                return BadRequest(new { error = $"Batch limit exceeded: max {MaxSignalsPerRequest} signals per request." });

            var (response, _) = await ProcessSignalsAsync(signals, adapter.AdapterID, persist: false, ct);
            return Ok(response);
        }
    }

    [HttpGet("ingest/stats")]
    [AllowAnonymous]
    [ProducesResponseType<PipelineStats>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> WebhookIngestStats(CancellationToken ct)
    {
        var adapter = await ResolveWebhookAdapter(Request, ct);
        if (adapter is null)
            return Unauthorized();

        return Ok(pipeline.GetStats());
    }

    // ── Time-Series Query API ─────────────────────────────────────────────

    /// <summary>
    /// Returns raw signal values for a given machine/tag in [from, to].
    /// </summary>
    [HttpGet("signals/history")]
    [RequirePermission(Permissions.IotRead)]
    [ProducesResponseType<IReadOnlyList<SignalHistoryPoint>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSignalHistory(
        [FromQuery] string machineCode,
        [FromQuery] string tagKey,
        [FromQuery] DateTimeOffset from,
        [FromQuery] DateTimeOffset to,
        [FromQuery] int limit = 500,
        CancellationToken ct = default)
    {
        var rows = await db.MachineSignalLogs
            .AsNoTracking()
            .Where(l => l.MachineCode == machineCode && l.TagKey == tagKey
                     && l.Timestamp >= from && l.Timestamp <= to)
            .OrderBy(l => l.Timestamp)
            .Take(limit)
            .Select(l => new SignalHistoryPoint(l.Value, l.Unit, l.Timestamp, false))
            .ToListAsync(ct);

        return Ok((IReadOnlyList<SignalHistoryPoint>)rows);
    }

    /// <summary>
    /// Returns aggregated signal values (1min or 1hr buckets) for charts/reports.
    /// </summary>
    [HttpGet("signals/aggregates")]
    [RequirePermission(Permissions.IotRead)]
    [ProducesResponseType<IReadOnlyList<SignalAggPoint>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSignalAggregates(
        [FromQuery] string machineCode,
        [FromQuery] string tagKey,
        [FromQuery] DateTimeOffset from,
        [FromQuery] DateTimeOffset to,
        [FromQuery] string resolution = "1min",
        CancellationToken ct = default)
    {
        if (resolution == "1hr")
        {
            var rows = await db.SignalAgg1hrs
                .AsNoTracking()
                .Where(a => a.MachineCode == machineCode && a.TagKey == tagKey
                         && a.BucketAt >= from && a.BucketAt <= to)
                .OrderBy(a => a.BucketAt)
                .Select(a => new SignalAggPoint(a.BucketAt, a.SampleCount, a.MinValue, a.MaxValue,
                    a.SampleCount > 0 ? a.SumValue / a.SampleCount : 0m, a.LastValue))
                .ToListAsync(ct);
            return Ok((IReadOnlyList<SignalAggPoint>)rows);
        }
        else
        {
            var rows = await db.SignalAgg1mins
                .AsNoTracking()
                .Where(a => a.MachineCode == machineCode && a.TagKey == tagKey
                         && a.BucketAt >= from && a.BucketAt <= to)
                .OrderBy(a => a.BucketAt)
                .Select(a => new SignalAggPoint(a.BucketAt, a.SampleCount, a.MinValue, a.MaxValue,
                    a.SampleCount > 0 ? a.SumValue / a.SampleCount : 0m, a.LastValue))
                .ToListAsync(ct);
            return Ok((IReadOnlyList<SignalAggPoint>)rows);
        }
    }

    /// <summary>
    /// Returns the global retention policy.
    /// </summary>
    [HttpGet("signals/retention")]
    [RequirePermission(Permissions.IotRead)]
    [ProducesResponseType<RetentionPolicyDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRetentionPolicy(CancellationToken ct)
    {
        var policy = await db.RetentionPolicies.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Scope == "GLOBAL", ct)
            ?? AeroMes.Domain.Iot.RetentionPolicy.CreateGlobal(30, 90, 730);

        return Ok(new RetentionPolicyDto(policy.RawRetentionDays, policy.Agg1minRetentionDays, policy.Agg1hrRetentionDays));
    }

    /// <summary>
    /// Updates the global retention policy.
    /// </summary>
    [HttpPut("signals/retention")]
    [RequirePermission(Permissions.IotWrite)]
    [ProducesResponseType<RetentionPolicyDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateRetentionPolicy(
        [FromBody] UpdateRetentionRequest request,
        CancellationToken ct)
    {
        var policy = await db.RetentionPolicies.FirstOrDefaultAsync(p => p.Scope == "GLOBAL", ct);
        if (policy is null)
        {
            policy = AeroMes.Domain.Iot.RetentionPolicy.CreateGlobal(request.RawRetentionDays, request.Agg1minRetentionDays, request.Agg1hrRetentionDays);
            db.RetentionPolicies.Add(policy);
        }
        else
        {
            policy.Update(request.RawRetentionDays, request.Agg1minRetentionDays, request.Agg1hrRetentionDays);
        }

        await db.SaveChangesAsync(ct);
        return Ok(new RetentionPolicyDto(policy.RawRetentionDays, policy.Agg1minRetentionDays, policy.Agg1hrRetentionDays));
    }

    // ── Auto-Resolution Signal Query ──────────────────────────────────────

    /// <summary>
    /// Returns signal time-series automatically selecting raw / 1-min / 1-hr tier based on range.
    /// Resolution rules: ≤1hr → raw; 1hr–24hr → 1-min aggregates; >24hr → 1-hr aggregates.
    /// </summary>
    [HttpGet("signals/query")]
    [RequirePermission(Permissions.IotRead)]
    [ProducesResponseType<IReadOnlyList<SignalAggPoint>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> QuerySignals(
        [FromQuery] string machineCode,
        [FromQuery] string tagKey,
        [FromQuery] DateTimeOffset from,
        [FromQuery] DateTimeOffset? to,
        [FromQuery] string resolution = "auto",
        [FromQuery] int limit = 1000,
        CancellationToken ct = default)
    {
        var toTime = to ?? DateTimeOffset.UtcNow;
        var span = toTime - from;

        var tier = resolution switch
        {
            "raw"  => "raw",
            "1min" => "1min",
            "1hr"  => "1hr",
            _      => span.TotalHours <= 1 ? "raw" : span.TotalHours <= 24 ? "1min" : "1hr",
        };

        if (tier == "raw")
        {
            var rows = await db.MachineSignalLogs
                .AsNoTracking()
                .Where(l => l.MachineCode == machineCode && l.TagKey == tagKey
                         && l.Timestamp >= from && l.Timestamp <= toTime)
                .OrderBy(l => l.Timestamp)
                .Take(limit)
                .Select(l => new SignalAggPoint(l.Timestamp, 1, l.Value, l.Value, l.Value, l.Value))
                .ToListAsync(ct);
            return Ok((IReadOnlyList<SignalAggPoint>)rows);
        }

        if (tier == "1hr")
        {
            var rows = await db.SignalAgg1hrs
                .AsNoTracking()
                .Where(a => a.MachineCode == machineCode && a.TagKey == tagKey
                         && a.BucketAt >= from && a.BucketAt <= toTime)
                .OrderBy(a => a.BucketAt)
                .Take(limit)
                .Select(a => new SignalAggPoint(a.BucketAt, a.SampleCount, a.MinValue, a.MaxValue,
                    a.SampleCount > 0 ? a.SumValue / a.SampleCount : 0m, a.LastValue))
                .ToListAsync(ct);
            return Ok((IReadOnlyList<SignalAggPoint>)rows);
        }
        else // 1min
        {
            var rows = await db.SignalAgg1mins
                .AsNoTracking()
                .Where(a => a.MachineCode == machineCode && a.TagKey == tagKey
                         && a.BucketAt >= from && a.BucketAt <= toTime)
                .OrderBy(a => a.BucketAt)
                .Take(limit)
                .Select(a => new SignalAggPoint(a.BucketAt, a.SampleCount, a.MinValue, a.MaxValue,
                    a.SampleCount > 0 ? a.SumValue / a.SampleCount : 0m, a.LastValue))
                .ToListAsync(ct);
            return Ok((IReadOnlyList<SignalAggPoint>)rows);
        }
    }

    // ── CSV Export ────────────────────────────────────────────────────────

    /// <summary>
    /// Streams raw signal logs as CSV for the given machine + tag in [from, to].
    /// </summary>
    [HttpGet("signals/export")]
    [RequirePermission(Permissions.IotRead)]
    public async Task ExportSignalsCsv(
        [FromQuery] string machineCode,
        [FromQuery] string tagKey,
        [FromQuery] DateTimeOffset from,
        [FromQuery] DateTimeOffset? to,
        CancellationToken ct = default)
    {
        var toTime = to ?? DateTimeOffset.UtcNow;
        Response.ContentType = "text/csv";
        Response.Headers.ContentDisposition = $"attachment; filename=\"signals_{machineCode}_{tagKey}.csv\"";

        var writer = Response.BodyWriter;
        await WriteLineAsync(writer, "Timestamp,Value,Unit,IsBadQuality", ct);

        var query = db.MachineSignalLogs
            .AsNoTracking()
            .Where(l => l.MachineCode == machineCode && l.TagKey == tagKey
                     && l.Timestamp >= from && l.Timestamp <= toTime)
            .OrderBy(l => l.Timestamp)
            .Select(l => new { l.Timestamp, l.Value, l.Unit, IsBadQuality = false });

        await foreach (var row in query.AsAsyncEnumerable().WithCancellation(ct))
        {
            await WriteLineAsync(writer,
                $"{row.Timestamp:O},{row.Value},{row.Unit ?? ""},{row.IsBadQuality}", ct);
        }

        await writer.FlushAsync(ct);

        static async Task WriteLineAsync(System.IO.Pipelines.PipeWriter w, string line, CancellationToken tok)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(line + "\n");
            await w.WriteAsync(bytes, tok);
        }
    }

    // ── Storage Stats ─────────────────────────────────────────────────────

    /// <summary>
    /// Returns row counts and date ranges for raw log and aggregate tables (admin).
    /// </summary>
    [HttpGet("storage/stats")]
    [RequirePermission(Permissions.IotRead)]
    [ProducesResponseType<IotStorageStatsDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStorageStats(CancellationToken ct)
    {
        var rawCount = await db.MachineSignalLogs.AsNoTracking().LongCountAsync(ct);
        var rawOldest = rawCount > 0
            ? await db.MachineSignalLogs.AsNoTracking().MinAsync(l => (DateTimeOffset?)l.Timestamp, ct)
            : null;
        var rawNewest = rawCount > 0
            ? await db.MachineSignalLogs.AsNoTracking().MaxAsync(l => (DateTimeOffset?)l.Timestamp, ct)
            : null;

        var agg1minCount = await db.SignalAgg1mins.AsNoTracking().LongCountAsync(ct);
        var agg1minOldest = agg1minCount > 0
            ? await db.SignalAgg1mins.AsNoTracking().MinAsync(a => (DateTimeOffset?)a.BucketAt, ct)
            : null;
        var agg1minNewest = agg1minCount > 0
            ? await db.SignalAgg1mins.AsNoTracking().MaxAsync(a => (DateTimeOffset?)a.BucketAt, ct)
            : null;

        var agg1hrCount = await db.SignalAgg1hrs.AsNoTracking().LongCountAsync(ct);
        var agg1hrOldest = agg1hrCount > 0
            ? await db.SignalAgg1hrs.AsNoTracking().MinAsync(a => (DateTimeOffset?)a.BucketAt, ct)
            : null;
        var agg1hrNewest = agg1hrCount > 0
            ? await db.SignalAgg1hrs.AsNoTracking().MaxAsync(a => (DateTimeOffset?)a.BucketAt, ct)
            : null;

        return Ok(new IotStorageStatsDto(
            new TableStatsDto("iot.MachineSignalLog", rawCount, rawOldest, rawNewest),
            new TableStatsDto("iot.SignalAgg_1min", agg1minCount, agg1minOldest, agg1minNewest),
            new TableStatsDto("iot.SignalAgg_1hr", agg1hrCount, agg1hrOldest, agg1hrNewest)
        ));
    }

    // ── Live Signals ──────────────────────────────────────────────────────

    /// <summary>
    /// Returns the most recent signal value per tag for a machine (for initial page load before SignalR takes over).
    /// </summary>
    [HttpGet("signals/live")]
    [RequirePermission(Permissions.IotRead)]
    [ProducesResponseType<IReadOnlyList<LiveSignalDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLiveSignals([FromQuery] string machineCode, CancellationToken ct)
    {
        var cutoff = DateTimeOffset.UtcNow.AddMinutes(-5);

        var recent = await db.MachineSignalLogs
            .AsNoTracking()
            .Where(l => l.MachineCode == machineCode && l.Timestamp >= cutoff)
            .OrderByDescending(l => l.Timestamp)
            .Take(2000)
            .ToListAsync(ct);

        // Group in memory — latest value per tag
        var latestPerTag = recent
            .GroupBy(l => l.TagKey)
            .Select(g => g.OrderByDescending(l => l.Timestamp).First())
            .Select(l => new LiveSignalDto(l.TagKey, l.Value, l.Unit, l.Timestamp))
            .ToList();

        return Ok((IReadOnlyList<LiveSignalDto>)latestPerTag);
    }

    // ── Adapter Health ────────────────────────────────────────────────────

    [HttpGet("adapters/health")]
    [RequirePermission(Permissions.IotRead)]
    [ProducesResponseType<IReadOnlyList<AdapterHealthDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAdaptersHealth(CancellationToken ct)
    {
        var healths = await healthRepository.GetAllAsync(ct);
        var result = healths.Select(ToDto).ToList();
        return Ok((IReadOnlyList<AdapterHealthDto>)result);
    }

    [HttpGet("adapters/{id:int}/health")]
    [RequirePermission(Permissions.IotRead)]
    [ProducesResponseType<AdapterHealthDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAdapterHealth(int id, [FromQuery] int logLimit = 50, CancellationToken ct = default)
    {
        var health = await healthRepository.GetByAdapterIdAsync(id, ct);
        if (health is null) return NotFound();

        var logs = await healthRepository.GetRecentLogsAsync(id, logLimit, ct);
        var logDtos = logs.Select(l => new AdapterHealthLogDto(l.EventId, l.EventType, l.EventAt, l.Details)).ToList();

        return Ok(new AdapterHealthDetailDto(ToDto(health), logDtos));
    }

    private static AdapterHealthDto ToDto(AdapterHealth h) =>
        new(h.AdapterId, h.MachineCode, h.AdapterType, h.Status.ToString(),
            h.LastConnectedAt, h.LastSignalAt, h.SignalRate1min,
            h.ErrorCount1hr, h.ReconnectAttempts, h.LastError, h.UpdatedAt);
}

public record CreateSignalTagRequest(string Key, string DisplayName, string Category, string DataType,
    string? DefaultUnit, decimal? TypicalMin, decimal? TypicalMax, string? Description);
public record UpdateSignalTagRequest(string DisplayName, string Category, string DataType,
    string? DefaultUnit, decimal? TypicalMin, decimal? TypicalMax, string? Description);
public record CreateAdapterRequest(string MachineCode, string AdapterType, string ConfigJson);
public record UpdateAdapterRequest(string ConfigJson);
public record AddSignalRequest(string TagKey, string DisplayName, string SourceAddress,
    double Scale = 1.0, double Offset = 0, double? QualityMin = null, double? QualityMax = null);
public record UpdateSignalRequest(string DisplayName, string SourceAddress,
    double Scale, double Offset, double? QualityMin, double? QualityMax);
public record CreateStateRuleRequest(string MachineCode, int Priority, string TargetState, string SignalTagKey,
    string Operator, double? ThresholdValue, double? Hysteresis, int MinDurationMs, string? Description);
public record UpdateStateRuleRequest(string TargetState, string SignalTagKey, string Operator,
    double? ThresholdValue, double? Hysteresis, int MinDurationMs, string? Description, bool IsActive);
public record ReorderStateRulesRequest(string MachineCode, List<int> OrderedRuleIds);
public record AdapterCreatedResult(int AdapterId);
public record SignalCreatedResult(int SignalId);
public record StateRuleCreatedResult(int RuleId);
public record MqttConnectionTestResult(bool Success, int LatencyMs, string BrokerHost, int BrokerPort, string? Error);
public record OpcBrowseRequest(string? NodeId);
public record OpcNodeInfo(string NodeId, string DisplayName, string NodeClass, string DataType);
public record OpcReadNodeRequest(string NodeId);
public record OpcNodeValue(string NodeId, string Value, string DataType, string StatusCode, DateTimeOffset SourceTimestamp);
public record ModbusReadRegistersRequest(string RegisterType, ushort StartAddress, int Count);
public record ModbusRegisterValue(ushort Address, string RawHex, float ParsedFloat);
public record ModbusReadRegistersResponse(
    string Host, int Port, string RegisterType, ushort StartAddress,
    int LatencyMs, List<ModbusRegisterValue> Registers);
// webhook ingest
public record WebhookSignalPayload(string MachineCode, string? TagKey, string? SourceAddress,
    decimal Value, string? Unit, DateTimeOffset? Timestamp);
public record WebhookMachineBundlePayload(string MachineCode, DateTimeOffset? Timestamp,
    Dictionary<string, decimal> Signals);
public record WebhookIngestResponse(int Accepted, int Rejected, IReadOnlyList<WebhookIngestError>? Errors);
public record WebhookIngestError(int Index, string? TagKey, string Reason);
// machine state engine
public record MachineStateSnapshotDto(string MachineCode, string CurrentState, string? PreviousState,
    DateTimeOffset StateChangedAt, string? TriggerTagKey, decimal? TriggerValue, bool IsStale);
public record MachineStateHistoryDto(long HistoryId, string MachineCode, string FromState, string ToState,
    DateTimeOffset TransitionAt, long DurationMs, string? TriggerTagKey, bool IsAutomatic);
public record StateOverrideRequest(string TargetState);
// time-series query DTOs
public record SignalHistoryPoint(decimal Value, string? Unit, DateTimeOffset Timestamp, bool IsBadQuality);
public record SignalAggPoint(DateTimeOffset BucketAt, int Count, decimal Min, decimal Max, decimal Avg, decimal Last);
public record RetentionPolicyDto(int RawRetentionDays, int Agg1minRetentionDays, int Agg1hrRetentionDays);
public record UpdateRetentionRequest(int RawRetentionDays, int Agg1minRetentionDays, int Agg1hrRetentionDays);
// storage stats
public record TableStatsDto(string TableName, long RowCount, DateTimeOffset? OldestRecord, DateTimeOffset? NewestRecord);
public record IotStorageStatsDto(TableStatsDto RawLog, TableStatsDto Agg1min, TableStatsDto Agg1hr);
// live signals
public record LiveSignalDto(string TagKey, decimal Value, string? Unit, DateTimeOffset Timestamp);
// adapter health
public record AdapterHealthDto(
    int AdapterId, string MachineCode, string AdapterType, string Status,
    DateTime? LastConnectedAt, DateTime? LastSignalAt, double SignalRate1min,
    int ErrorCount1hr, int ReconnectAttempts, string? LastError, DateTime UpdatedAt);
public record AdapterHealthLogDto(long EventId, string EventType, DateTime EventAt, string? Details);
public record AdapterHealthDetailDto(AdapterHealthDto Health, List<AdapterHealthLogDto> Logs);
