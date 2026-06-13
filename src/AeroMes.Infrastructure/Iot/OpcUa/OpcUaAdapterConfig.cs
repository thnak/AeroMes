namespace AeroMes.Infrastructure.Iot.OpcUa;

public class OpcUaAdapterConfig
{
    public string ServerUrl { get; set; } = "opc.tcp://localhost:4840";
    public string SecurityMode { get; set; } = "None";    // None | Sign | SignAndEncrypt
    public string SecurityPolicy { get; set; } = "None";  // None | Basic256Sha256 | ...
    public string AuthMode { get; set; } = "Anonymous";   // Anonymous | UsernamePassword | Certificate
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? CertificatePath { get; set; }
    public string? CertificatePassword { get; set; }
    public int SessionTimeoutMs { get; set; } = 30_000;
    public int PublishingIntervalMs { get; set; } = 500;
    public string SubscriptionMode { get; set; } = "Subscription"; // Subscription | Poll
    public int PollIntervalMs { get; set; } = 1000;
    public int MaxKeepAliveCount { get; set; } = 10;
    public int ReconnectDelayMs { get; set; } = 5000;
}
