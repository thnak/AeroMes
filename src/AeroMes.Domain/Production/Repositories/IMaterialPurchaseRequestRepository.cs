namespace AeroMes.Domain.Production.Repositories;

public record MaterialPurchaseRequestLineDto(
    int LineID,
    string MaterialCode,
    string MaterialName,
    string UnitOfMeasure,
    decimal RequiredQty,
    decimal? CalculatedQty,
    decimal? Length,
    decimal? Width,
    decimal? Height,
    decimal? Radius,
    decimal? Weight);

public record MaterialPurchaseRequestDto(
    int RequestID,
    string RequestNumber,
    DateOnly CreationDate,
    string Requestor,
    string? RequestingUnit,
    DateOnly? Deadline,
    string? ProcurementPurpose,
    string Status,
    string SourceType,
    int? SourceReferenceId,
    string? SalesOrderCode,
    DateTime CreatedAt,
    IReadOnlyList<MaterialPurchaseRequestLineDto> Lines);

public interface IMaterialPurchaseRequestRepository
{
    Task AddAsync(MaterialPurchaseRequest request, CancellationToken ct = default);
    Task<MaterialPurchaseRequest?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<bool> NumberExistsAsync(string requestNumber, CancellationToken ct = default);

    Task<(IReadOnlyList<MaterialPurchaseRequestDto> Items, int Total)> GetListAsync(
        PurchaseRequestStatus? status,
        PurchaseRequestSourceType? sourceType,
        string? requestingUnit,
        DateTime? fromDate,
        DateTime? toDate,
        int page, int pageSize,
        CancellationToken ct = default);

    Task SaveChangesAsync(CancellationToken ct = default);
}
