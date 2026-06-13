namespace AeroMes.Domain.Sop.Repositories;

public interface ISopRepository
{
    // SOP Documents
    Task<List<SopDocument>> GetDocumentsAsync(int? routingStepId, string? productCode,
        string? status, CancellationToken ct);
    Task<SopDocument?> GetDocumentByIdAsync(int id, CancellationToken ct);
    Task<SopDocument?> GetActiveForStepAsync(int routingStepId, string? productCode, CancellationToken ct);
    void AddDocument(SopDocument doc);

    // Checksheet Instances
    Task<ChecksheetInstance?> GetInstanceForJobAsync(long jobId, CancellationToken ct);
    Task<ChecksheetInstance?> GetInstanceByIdAsync(long instanceId, CancellationToken ct);
    Task<List<ChecksheetInstance>> GetInstancesForWorkOrderAsync(long workOrderId, CancellationToken ct);
    void AddInstance(ChecksheetInstance instance);

    // CheckItemResults
    Task<CheckItemResult?> GetItemResultAsync(long instanceId, int checkItemId, CancellationToken ct);
    void UpdateItemResult(CheckItemResult result);
}
