namespace AeroMes.Infrastructure.Iot.Modbus;

public class ModbusAdapterConfig
{
    public string Host { get; set; } = "127.0.0.1";
    public int Port { get; set; } = 502;
    public byte UnitId { get; set; } = 1;
    public int PollIntervalMs { get; set; } = 1000;
    public int TimeoutMs { get; set; } = 2000;
    public int MaxRetries { get; set; } = 3;
    public int RetryDelayMs { get; set; } = 500;
    public string ByteOrder { get; set; } = "BigEndian";     // BigEndian | LittleEndian
    public string WordOrder { get; set; } = "HighWordFirst"; // HighWordFirst | LowWordFirst
}
