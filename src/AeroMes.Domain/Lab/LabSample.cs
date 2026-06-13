namespace AeroMes.Domain.Lab;

public class LabSample
{
    public long SampleId { get; private set; }
    public int RequestId { get; private set; }
    public string SampleCode { get; private set; } = string.Empty;
    public string ConditionOnReceipt { get; private set; } = "OK";
    public string ReceivedBy { get; private set; } = string.Empty;
    public DateTimeOffset ReceivedAt { get; private set; }
    public string? StorageLocation { get; private set; }
    public DateTimeOffset? DisposedAt { get; private set; }
    public string? DisposalMethod { get; private set; }

    private LabSample() { }

    public static LabSample Create(int requestId, string sampleCode, string conditionOnReceipt,
        string receivedBy, string? storageLocation)
        => new()
        {
            RequestId = requestId, SampleCode = sampleCode,
            ConditionOnReceipt = conditionOnReceipt, ReceivedBy = receivedBy,
            ReceivedAt = DateTimeOffset.UtcNow, StorageLocation = storageLocation,
        };

    public void Dispose(string disposalMethod)
    {
        DisposedAt = DateTimeOffset.UtcNow;
        DisposalMethod = disposalMethod;
    }
}
