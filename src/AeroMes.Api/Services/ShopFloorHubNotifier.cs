using AeroMes.Api.Hubs;
using AeroMes.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace AeroMes.Api.Services;

public class ShopFloorHubNotifier(IHubContext<ShopFloorHub> hub) : IShopFloorNotifier
{
    public Task MachineStatusChangedAsync(string machineCode, string status, DateTimeOffset timestamp, CancellationToken ct = default) =>
        hub.Clients.Groups("factory", $"machine:{machineCode}").SendAsync(
            "MachineStatusChanged",
            new MachineStatusChangedPayload(machineCode, status, timestamp),
            ct);

    public Task WorkOrderProgressUpdatedAsync(int workOrderId, string woCode, int actualOk, int actualNg, double completionPct, CancellationToken ct = default) =>
        hub.Clients.Group("factory").SendAsync(
            "WorkOrderProgressUpdated",
            new WorkOrderProgressPayload(workOrderId, woCode, actualOk, actualNg, completionPct),
            ct);

    public Task WorkOrderStartedAsync(int workOrderId, string woCode, int workCenterId, CancellationToken ct = default) =>
        hub.Clients.Groups("factory", $"wc:{workCenterId}").SendAsync(
            "WorkOrderStarted",
            new WorkOrderStartedPayload(workOrderId, woCode, workCenterId),
            ct);

    public Task WorkOrderCompletedAsync(int workOrderId, string woCode, CancellationToken ct = default) =>
        hub.Clients.Group("factory").SendAsync(
            "WorkOrderCompleted",
            new WorkOrderCompletedPayload(workOrderId, woCode),
            ct);
}

public record MachineStatusChangedPayload(string MachineCode, string Status, DateTimeOffset Timestamp);
public record WorkOrderProgressPayload(int WorkOrderId, string WoCode, int ActualOk, int ActualNg, double CompletionPct);
public record WorkOrderStartedPayload(int WorkOrderId, string WoCode, int WorkCenterId);
public record WorkOrderCompletedPayload(int WorkOrderId, string WoCode);
