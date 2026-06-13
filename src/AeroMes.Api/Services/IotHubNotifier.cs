using AeroMes.Api.Hubs;
using AeroMes.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace AeroMes.Api.Services;

public interface IIotHubNotifier : IIotSignalNotifier { }

public class IotHubNotifier(IHubContext<IotHub> hub) : IIotHubNotifier
{
    public Task SendSignalAsync(string machineCode, string tagKey, decimal value, string? unit,
        DateTimeOffset timestamp, bool isBadQuality, CancellationToken ct) =>
        hub.Clients.Group($"machine:{machineCode}").SendAsync("MachineSignalUpdated",
            new MachineSignalHubPayload(machineCode, tagKey, value, unit, timestamp, isBadQuality), ct);

    public Task SendStateChangedAsync(string machineCode, string newState, string? previousState,
        DateTimeOffset changedAt, CancellationToken ct) =>
        hub.Clients.Group($"machine:{machineCode}").SendAsync("MachineStateChanged",
            new MachineStateHubPayload(machineCode, newState, previousState, changedAt), ct);
}

public record MachineSignalHubPayload(
    string MachineCode, string TagKey, decimal Value, string? Unit,
    DateTimeOffset Timestamp, bool IsBadQuality);

public record MachineStateHubPayload(
    string MachineCode, string NewState, string? PreviousState, DateTimeOffset ChangedAt);
