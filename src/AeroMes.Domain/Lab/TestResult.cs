namespace AeroMes.Domain.Lab;

public class TestResult
{
    public long ResultId { get; private set; }
    public int RequestId { get; private set; }
    public long SampleId { get; private set; }
    public int TestMethodId { get; private set; }
    public decimal? MeasuredValue { get; private set; }
    public string? AttributeResult { get; private set; }
    public bool? IsWithinSpec { get; private set; }
    public string? InstrumentCode { get; private set; }
    public string TestedBy { get; private set; } = string.Empty;
    public DateTimeOffset TestedAt { get; private set; }
    public string? ReviewedBy { get; private set; }
    public DateTimeOffset? ReviewedAt { get; private set; }
    public string? Notes { get; private set; }

    private TestResult() { }

    public static TestResult Record(int requestId, long sampleId, int testMethodId,
        decimal? measuredValue, string? attributeResult, bool? isWithinSpec,
        string? instrumentCode, string testedBy, string? notes)
        => new()
        {
            RequestId = requestId, SampleId = sampleId, TestMethodId = testMethodId,
            MeasuredValue = measuredValue, AttributeResult = attributeResult,
            IsWithinSpec = isWithinSpec, InstrumentCode = instrumentCode,
            TestedBy = testedBy, TestedAt = DateTimeOffset.UtcNow, Notes = notes,
        };

    public void Review(string reviewedBy)
    {
        ReviewedBy = reviewedBy;
        ReviewedAt = DateTimeOffset.UtcNow;
    }
}
