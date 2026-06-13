namespace AeroMes.Domain.Quality.Repositories;

public interface IDefectLifecycleRepository
{
    // DefectEntry
    Task<IReadOnlyList<DefectEntry>> GetEntriesAsync(long? workOrderId, string? status, CancellationToken ct);
    Task<DefectEntry?> GetEntryByIdAsync(int id, CancellationToken ct);
    void AddEntry(DefectEntry entry);

    // RepairOrder
    Task<IReadOnlyList<RepairOrder>> GetRepairOrdersAsync(string? status, CancellationToken ct);
    Task<RepairOrder?> GetRepairOrderByIdAsync(int id, CancellationToken ct);
    Task<int> GetNextRepairOrderSeqAsync(CancellationToken ct);
    void AddRepairOrder(RepairOrder order);
}
