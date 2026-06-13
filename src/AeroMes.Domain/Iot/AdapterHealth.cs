namespace AeroMes.Domain.Iot;

public class AdapterHealth
{
    public int AdapterId { get; private set; }
    public string MachineCode { get; private set; } = string.Empty;
    public string AdapterType { get; private set; } = string.Empty;
    public AdapterHealthStatus Status { get; private set; } = AdapterHealthStatus.Unknown;
    public DateTime? LastConnectedAt { get; private set; }
    public DateTime? LastSignalAt { get; private set; }
    public double SignalRate1min { get; private set; }
    public int ErrorCount1hr { get; private set; }
    public int ReconnectAttempts { get; private set; }
    public string? LastError { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private AdapterHealth() { }

    public static AdapterHealth Create(int adapterId, string machineCode, string adapterType) =>
        new()
        {
            AdapterId = adapterId,
            MachineCode = machineCode,
            AdapterType = adapterType,
            UpdatedAt = DateTime.UtcNow,
        };

    public bool MarkConnected()
    {
        if (Status == AdapterHealthStatus.Connected) return false;
        Status = AdapterHealthStatus.Connected;
        LastConnectedAt = DateTime.UtcNow;
        ReconnectAttempts = 0;
        LastError = null;
        UpdatedAt = DateTime.UtcNow;
        return true;
    }

    public bool MarkDisconnected(string? error)
    {
        if (Status == AdapterHealthStatus.Disconnected) return false;
        Status = AdapterHealthStatus.Disconnected;
        LastError = error;
        UpdatedAt = DateTime.UtcNow;
        return true;
    }

    public bool MarkDegraded(string? error)
    {
        if (Status == AdapterHealthStatus.Degraded) return false;
        Status = AdapterHealthStatus.Degraded;
        LastError = error;
        UpdatedAt = DateTime.UtcNow;
        return true;
    }

    public bool MarkUnknown()
    {
        if (Status == AdapterHealthStatus.Unknown) return false;
        Status = AdapterHealthStatus.Unknown;
        UpdatedAt = DateTime.UtcNow;
        return true;
    }

    public void UpdateSignalMetrics(DateTime lastSignalAt, double rate1min)
    {
        LastSignalAt = lastSignalAt;
        SignalRate1min = rate1min;
        UpdatedAt = DateTime.UtcNow;
    }

    public void IncrementError(string? error)
    {
        ErrorCount1hr++;
        LastError = error;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ResetErrorCount()
    {
        ErrorCount1hr = 0;
        UpdatedAt = DateTime.UtcNow;
    }

    public void IncrementReconnect()
    {
        ReconnectAttempts++;
        UpdatedAt = DateTime.UtcNow;
    }
}

public enum AdapterHealthStatus { Connected, Degraded, Disconnected, Unknown }
