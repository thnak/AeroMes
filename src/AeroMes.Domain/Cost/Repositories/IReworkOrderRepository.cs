namespace AeroMes.Domain.Cost.Repositories;

public record ReworkOrderDto(
    int ReworkID, string ReworkCode, int SourceWOID, long? ScrapTxID,
    string ProductCode, int ReworkQty, int? ReworkStepFromId,
    string Status, decimal ActMaterialCost, decimal ActLaborCost,
    decimal ActMachineCost, decimal ActTotalReworkCost, DateTime CreatedAt);

public interface IReworkOrderRepository
{
    Task AddAsync(ReworkOrder order, CancellationToken ct);
    Task<ReworkOrder?> GetByIdAsync(int id, CancellationToken ct);
    Task<bool> CodeExistsAsync(string code, CancellationToken ct);
    Task<(IReadOnlyList<ReworkOrderDto> Items, int Total)> GetListAsync(
        ReworkStatus? status, string? productCode, int page, int pageSize, CancellationToken ct);
}
