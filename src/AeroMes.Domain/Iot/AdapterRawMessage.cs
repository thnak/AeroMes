namespace AeroMes.Domain.Iot;

public record AdapterRawMessage(
    int AdapterId, string SourceAddress, decimal RawValue,
    DateTimeOffset Timestamp, string Source);
