namespace AeroMes.Domain.Integration.Repositories;

public record SalesOrderSummaryDto(
    int SOID,
    string SOCode,
    string? CustomerCode,
    string? CustomerName,
    DateTime OrderDate,
    DateTime? DeliveryDate,
    string Status,
    string SyncSource,
    string? FacilityCode,
    bool IsConfirmed,
    string? CreatedBy,
    DateTime CreatedAt);

public interface ISalesOrderRepository
{
    Task<SalesOrder?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<SalesOrder?> GetByIdWithLinesAsync(int id, CancellationToken ct = default);
    Task<SalesOrder?> GetByCodeAsync(string soCode, CancellationToken ct = default);
    Task<IReadOnlyList<SalesOrder>> GetFilteredAsync(
        string? soCode, SalesOrderStatus? status,
        DateTime? from, DateTime? to, CancellationToken ct = default);
    Task<IReadOnlyList<SalesOrderSummaryDto>> GetListAsync(
        string? soCode, string? status, bool includeUnconfirmed,
        DateTime? from, DateTime? to, CancellationToken ct = default);
    Task<string> NextSoCodeAsync(CancellationToken ct = default);
    Task AddAsync(SalesOrder entity, CancellationToken ct = default);
}
