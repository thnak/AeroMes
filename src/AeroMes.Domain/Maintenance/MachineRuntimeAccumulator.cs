namespace AeroMes.Domain.Maintenance;

public class MachineRuntimeAccumulator
{
    public string MachineCode { get; private set; } = string.Empty;
    public long CumulativeRuntimeMinutes { get; private set; }
    public DateTime LastUpdatedAt { get; private set; }

    private MachineRuntimeAccumulator() { }

    public static MachineRuntimeAccumulator Create(string machineCode)
        => new() { MachineCode = machineCode.Trim().ToUpperInvariant(), LastUpdatedAt = DateTime.UtcNow };

    public void AddMinutes(long minutes)
    {
        CumulativeRuntimeMinutes += minutes;
        LastUpdatedAt = DateTime.UtcNow;
    }
}
