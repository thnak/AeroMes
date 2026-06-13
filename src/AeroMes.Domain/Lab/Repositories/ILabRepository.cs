namespace AeroMes.Domain.Lab.Repositories;

public interface ILabRepository
{
    // Test methods
    Task<List<TestMethod>> GetMethodsAsync(string? category, bool? isActive, CancellationToken ct);
    Task<TestMethod?> GetMethodByIdAsync(int id, CancellationToken ct);
    void AddMethod(TestMethod method);

    // Test panels
    Task<List<TestPanel>> GetPanelsAsync(bool? isActive, CancellationToken ct);
    Task<TestPanel?> GetPanelByIdAsync(int id, CancellationToken ct);
    void AddPanel(TestPanel panel);

    // Lab requests
    Task<List<LabRequest>> GetRequestsAsync(string? status, string? priority, string? requestType,
        string? productCode, DateTimeOffset? from, DateTimeOffset? to, CancellationToken ct);
    Task<LabRequest?> GetRequestByIdAsync(int id, CancellationToken ct);
    void AddRequest(LabRequest request);

    // Sequence for request/report numbers
    Task<int> GetNextRequestSeqAsync(int year, CancellationToken ct);
    Task<int> GetNextReportSeqAsync(int year, CancellationToken ct);

    // Samples
    Task<List<LabSample>> GetSamplesForRequestAsync(int requestId, CancellationToken ct);
    Task<LabSample?> GetSampleByIdAsync(long sampleId, CancellationToken ct);
    void AddSample(LabSample sample);

    // Results
    Task<List<TestResult>> GetResultsForRequestAsync(int requestId, CancellationToken ct);
    Task<TestResult?> GetResultByIdAsync(long resultId, CancellationToken ct);
    void AddResult(TestResult result);
    void UpdateResult(TestResult result);

    // Reports
    Task<LabReport?> GetReportForRequestAsync(int requestId, CancellationToken ct);
    Task<LabReport?> GetReportByIdAsync(int reportId, CancellationToken ct);
    void AddReport(LabReport report);
}
