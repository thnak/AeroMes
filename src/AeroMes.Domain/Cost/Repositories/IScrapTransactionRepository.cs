namespace AeroMes.Domain.Cost.Repositories;

public record ScrapParetoDto(int? DefectCodeId, string? DefectCode, string? DefectName, int TotalQty, decimal TotalCost);
public record ScrapTransactionDto(
    long ScrapTxID, int WOID, long? LogID, int? DefectCodeId,
    string ProductCode, string? LotNumber, int ScrapQty,
    decimal MaterialCostPerUnit, decimal LaborCostSunk, decimal MachineCostSunk,
    decimal TotalScrapCost, string DisposalMethod, DateTime ScrapAt, string? Notes);

public interface IScrapTransactionRepository
{
    Task AddAsync(ScrapTransaction tx, CancellationToken ct);
    Task<ScrapTransaction?> GetByIdAsync(long id, CancellationToken ct);
    Task<(IReadOnlyList<ScrapTransactionDto> Items, int Total)> GetListAsync(
        int? woid, string? productCode, DateTime? from, DateTime? to,
        int page, int pageSize, CancellationToken ct);
    Task<IReadOnlyList<ScrapParetoDto>> GetParetoAsync(DateTime from, DateTime to, int? workCenterId, CancellationToken ct);
}
