namespace AeroMes.Domain.Iot;

public record MachineSignalMessage(
    string MachineCode, string TagKey, decimal Value, string? Unit,
    DateTimeOffset Timestamp, string Source, bool IsBadQuality);
