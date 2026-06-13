namespace AeroMes.Application.Interfaces;

public interface IIotSignalNotifier
{
    Task SendSignalAsync(string machineCode, string tagKey, decimal value, string? unit,
        DateTimeOffset timestamp, bool isBadQuality, CancellationToken ct);

    Task SendStateChangedAsync(string machineCode, string newState, string? previousState,
        DateTimeOffset changedAt, CancellationToken ct);
}
