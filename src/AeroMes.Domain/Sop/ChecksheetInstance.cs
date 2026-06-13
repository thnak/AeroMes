namespace AeroMes.Domain.Sop;

public class ChecksheetInstance
{
    public long InstanceId { get; private set; }
    public int SopId { get; private set; }
    public long JobId { get; private set; }
    public long WorkOrderId { get; private set; }
    public string Status { get; private set; } = "IN_PROGRESS";
    public DateTimeOffset StartedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public string OperatorCode { get; private set; } = string.Empty;
    public string? OverrideReason { get; private set; }

    private readonly List<CheckItemResult> _results = [];
    public IReadOnlyList<CheckItemResult> Results => _results;

    private ChecksheetInstance() { }

    public static ChecksheetInstance Create(int sopId, long jobId, long workOrderId, string operatorCode)
        => new()
        {
            SopId = sopId, JobId = jobId, WorkOrderId = workOrderId,
            OperatorCode = operatorCode, StartedAt = DateTimeOffset.UtcNow,
        };

    public void AddResults(IEnumerable<CheckItemResult> results)
    {
        _results.Clear();
        _results.AddRange(results);
    }

    public void Complete()
    {
        Status = "COMPLETED";
        CompletedAt = DateTimeOffset.UtcNow;
    }

    public void Override(string reason)
    {
        Status = "OVERRIDDEN";
        OverrideReason = reason;
        CompletedAt = DateTimeOffset.UtcNow;
    }
}

public class CheckItemResult
{
    public long ResultId { get; private set; }
    public long InstanceId { get; private set; }
    public int CheckItemId { get; private set; }
    public string Result { get; private set; } = "IN_PROGRESS";
    public string? CompletedBy { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public string CompletionSource { get; private set; } = "OPERATOR";
    public decimal? MeasuredValue { get; private set; }
    public string? Notes { get; private set; }
    public string? PhotoUrl { get; private set; }

    private CheckItemResult() { }

    public static CheckItemResult Pending(long instanceId, int checkItemId)
        => new() { InstanceId = instanceId, CheckItemId = checkItemId };

    public void Complete(string result, string completedBy, string completionSource,
        decimal? measuredValue, string? notes, string? photoUrl)
    {
        Result = result;
        CompletedBy = completedBy;
        CompletedAt = DateTimeOffset.UtcNow;
        CompletionSource = completionSource;
        MeasuredValue = measuredValue;
        Notes = notes;
        PhotoUrl = photoUrl;
    }
}
