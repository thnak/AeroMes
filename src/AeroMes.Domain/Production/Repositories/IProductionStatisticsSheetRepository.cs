namespace AeroMes.Domain.Production.Repositories;

public interface IProductionStatisticsSheetRepository
{
    Task<ProductionStatisticsSheet?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<ProductionStatisticsSheet>> GetFilteredAsync(
        int? poId, int? mpoId,
        StatisticsSheetType? sheetType,
        StatisticsSheetStatus? status,
        DateOnly? from, DateOnly? to,
        CancellationToken ct = default);
    Task<IReadOnlyList<ProductionStatisticsSheet>> GetByPoIdAsync(int poId, CancellationToken ct = default);
    Task AddAsync(ProductionStatisticsSheet entity, CancellationToken ct = default);
    Task<int> CountAsync(CancellationToken ct = default);
}
