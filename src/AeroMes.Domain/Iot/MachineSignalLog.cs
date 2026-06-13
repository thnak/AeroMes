namespace AeroMes.Domain.Iot;

public class MachineSignalLog
{
    public long LogId { get; private set; }
    public string MachineCode { get; private set; } = "";
    public string TagKey { get; private set; } = "";
    public decimal Value { get; private set; }
    public string? Unit { get; private set; }
    public DateTimeOffset Timestamp { get; private set; }
    public string Source { get; private set; } = "";

    private MachineSignalLog() { }

    public static MachineSignalLog FromMessage(MachineSignalMessage msg)
        => new()
        {
            MachineCode = msg.MachineCode,
            TagKey = msg.TagKey,
            Value = msg.Value,
            Unit = msg.Unit,
            Timestamp = msg.Timestamp,
            Source = msg.Source
        };
}
