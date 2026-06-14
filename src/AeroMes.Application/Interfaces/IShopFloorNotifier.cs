namespace AeroMes.Application.Interfaces;

public interface IShopFloorNotifier
{
    Task MachineStatusChangedAsync(string machineCode, string status, DateTimeOffset timestamp, CancellationToken ct = default);
    Task WorkOrderProgressUpdatedAsync(int workOrderId, string woCode, int actualOk, int actualNg, double completionPct, CancellationToken ct = default);
    Task WorkOrderStartedAsync(int workOrderId, string woCode, int workCenterId, CancellationToken ct = default);
    Task WorkOrderCompletedAsync(int workOrderId, string woCode, CancellationToken ct = default);
}
