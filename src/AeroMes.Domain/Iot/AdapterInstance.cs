using AeroMes.Domain.Common;

namespace AeroMes.Domain.Iot;

public class AdapterInstance : AuditableEntity
{
    public int AdapterID { get; private set; }
    public string MachineCode { get; private set; } = string.Empty;
    public AdapterType AdapterType { get; private set; }
    public string ConfigJson { get; private set; } = "{}";
    public AdapterStatus Status { get; private set; } = AdapterStatus.Unknown;
    public bool IsEnabled { get; private set; } = true;
    public DateTime? LastSignalAt { get; private set; }
    public string? WebhookApiKey { get; private set; }

    private readonly List<SignalMapping> _signals = [];
    public IReadOnlyCollection<SignalMapping> Signals => _signals.AsReadOnly();

    private AdapterInstance() { }

    public static AdapterInstance Create(string machineCode, AdapterType type, string configJson, string? createdBy)
    {
        var adapter = new AdapterInstance
        {
            MachineCode = machineCode,
            AdapterType = type,
            ConfigJson = configJson,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
        };
        if (type == AdapterType.Webhook)
            adapter.WebhookApiKey = Guid.NewGuid().ToString("N");
        return adapter;
    }

    public void UpdateConfig(string configJson, string updatedBy) { ConfigJson = configJson; Touch(updatedBy); }
    public void Enable(string updatedBy) { IsEnabled = true; Touch(updatedBy); }
    public void Disable(string updatedBy) { IsEnabled = false; Touch(updatedBy); }
    public void SetStatus(AdapterStatus status)
    {
        Status = status;
        LastSignalAt = status == AdapterStatus.Connected ? DateTime.UtcNow : LastSignalAt;
    }
}

public enum AdapterType { Mqtt, OpcUa, Modbus, Webhook }
public enum AdapterStatus { Connected, Disconnected, Degraded, Unknown }
