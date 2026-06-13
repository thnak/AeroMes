namespace AeroMes.Domain.Production.Repositories;

public record DisassemblyOrderDto(
    int DisassemblyOrderID,
    string OrderCode,
    string OrderType,
    string SourceProductCode,
    int DisassemblyBomId,
    decimal SourceQty,
    string Status,
    DateTime? Deadline,
    string? Notes,
    DateTime CreatedAt,
    DateTime? StartedAt,
    DateTime? CompletedAt,
    IReadOnlyList<DisassemblyRecoveredLineDto> RecoveredLines);

public record DisassemblyRecoveredLineDto(
    string ProductCode,
    decimal ExpectedQty,
    decimal ActualQty);

public interface IDisassemblyOrderRepository
{
    Task AddAsync(DisassemblyOrder order, CancellationToken ct = default);
    Task<DisassemblyOrder?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<bool> CodeExistsAsync(string code, CancellationToken ct = default);
    Task<(IReadOnlyList<DisassemblyOrderDto> Items, int Total)> GetListAsync(
        string? sourceProductCode, DisassemblyOrderStatus? status,
        DateTime? fromDate, DateTime? toDate,
        int page, int pageSize, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
