namespace AeroMes.Infrastructure.Iot.Mqtt;

public class MqttAdapterConfig
{
    public string BrokerHost { get; set; } = "localhost";
    public int BrokerPort { get; set; } = 1883;
    public bool UseTls { get; set; } = false;
    public string ClientId { get; set; } = "aeromes";
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string TopicPrefix { get; set; } = "factory/machines";
    public int QoS { get; set; } = 1;
    public bool CleanSession { get; set; } = false;
    public int KeepAliveSeconds { get; set; } = 60;
    public string PayloadFormat { get; set; } = "JSON";      // JSON | PLAIN_NUMBER | CSV
    public string JsonValuePath { get; set; } = "$.value";
    public string? JsonTimestampPath { get; set; } = "$.ts";
    public string TimestampFormat { get; set; } = "unix_ms"; // unix_ms | unix_s | iso8601 | server
    public int ReconnectDelayMs { get; set; } = 3000;
    public int MaxReconnectAttempts { get; set; } = 0;       // 0 = unlimited
}
