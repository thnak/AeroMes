namespace AeroMes.Domain.Iot;

public class SignalAgg1hr
{
    public long BucketId { get; private set; }
    public string MachineCode { get; private set; } = "";
    public string TagKey { get; private set; } = "";
    public DateTimeOffset BucketAt { get; private set; }  // truncated to hour
    public int SampleCount { get; private set; }
    public decimal SumValue { get; private set; }
    public decimal MinValue { get; private set; }
    public decimal MaxValue { get; private set; }
    public decimal LastValue { get; private set; }
    public decimal FirstValue { get; private set; }

    private SignalAgg1hr() { }

    public static SignalAgg1hr Create(string machineCode, string tagKey, DateTimeOffset bucketAt,
        int count, decimal sum, decimal min, decimal max, decimal first, decimal last)
        => new() { MachineCode = machineCode, TagKey = tagKey, BucketAt = bucketAt,
                   SampleCount = count, SumValue = sum, MinValue = min, MaxValue = max,
                   FirstValue = first, LastValue = last };
}
