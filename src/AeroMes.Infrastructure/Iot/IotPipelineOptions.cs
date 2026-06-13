namespace AeroMes.Infrastructure.Iot;

public class IotPipelineOptions
{
    public int ChannelCapacity { get; set; } = 50_000;
    public int BatchSize { get; set; } = 200;
    public int BatchFlushIntervalMs { get; set; } = 500;
    public bool PersistBadQuality { get; set; } = false;
    public double DefaultDeadbandPercent { get; set; } = 0.0;
    public int DefaultMinIntervalMs { get; set; } = 100;
}
