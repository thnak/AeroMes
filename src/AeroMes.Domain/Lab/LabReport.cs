namespace AeroMes.Domain.Lab;

public class LabReport
{
    public int ReportId { get; private set; }
    public string ReportNo { get; private set; } = string.Empty;
    public int RequestId { get; private set; }
    public string OverallResult { get; private set; } = string.Empty;
    public string Conclusion { get; private set; } = string.Empty;
    public string IssuedBy { get; private set; } = string.Empty;
    public DateTimeOffset IssuedAt { get; private set; }
    public string? CustomerCode { get; private set; }
    public string? DocumentUrl { get; private set; }

    private LabReport() { }

    public static LabReport Issue(string reportNo, int requestId, string overallResult,
        string conclusion, string issuedBy, string? customerCode)
        => new()
        {
            ReportNo = reportNo, RequestId = requestId, OverallResult = overallResult,
            Conclusion = conclusion, IssuedBy = issuedBy, IssuedAt = DateTimeOffset.UtcNow,
            CustomerCode = customerCode,
        };
}
