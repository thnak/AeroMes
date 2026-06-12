namespace AeroMes.Domain.Integration.Repositories;

public interface ISalesOrderRepository
{
    Task<SalesOrder?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<SalesOrder>> GetFilteredAsync(
        string? soCode, SalesOrderStatus? status,
        DateTime? from, DateTime? to, CancellationToken ct = default);
}
