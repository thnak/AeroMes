namespace AeroMes.Domain.Production.Repositories;

public record PackagingBomDto(
    int PackagingBomID,
    string ProductCode,
    int Version,
    bool IsActive,
    string? Notes,
    DateTime CreatedAt,
    IReadOnlyList<PackagingBomLineDto> Lines);

public record PackagingBomLineDto(
    int LineID,
    string MaterialCode,
    decimal Quantity,
    string UnitCode,
    string? Notes);

public record PackagingOrderDto(
    int PackagingOrderID,
    int WOID,
    int PackagingBomID,
    string ProductCode,
    string IdentificationCode,
    decimal PlannedQty,
    decimal PackagedQty,
    string Status,
    string? Notes,
    DateTime CreatedAt,
    DateTime? CompletedAt);

public interface IPackagingRepository
{
    // BOM
    Task AddBomAsync(PackagingBom bom, CancellationToken ct = default);
    Task<PackagingBom?> GetBomByIdAsync(int id, CancellationToken ct = default);
    Task<PackagingBom?> GetActiveBomForProductAsync(string productCode, CancellationToken ct = default);
    Task<IReadOnlyList<PackagingBomDto>> GetBomsAsync(string? productCode, CancellationToken ct = default);

    // Order
    Task AddOrderAsync(PackagingOrder order, CancellationToken ct = default);
    Task<PackagingOrder?> GetOrderByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<PackagingOrderDto>> GetOrdersAsync(int? woid, PackagingOrderStatus? status, CancellationToken ct = default);

    // Label
    Task<PackagingLabel?> GetLabelByIdAsync(int labelId, CancellationToken ct = default);
    Task MarkLabelPrintedAsync(int labelId, DateTime printedAt, CancellationToken ct = default);

    Task SaveChangesAsync(CancellationToken ct = default);
}
