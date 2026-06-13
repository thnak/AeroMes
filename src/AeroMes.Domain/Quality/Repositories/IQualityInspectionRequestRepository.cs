namespace AeroMes.Domain.Quality.Repositories;

public record InspectionRequestDto(
    int RequestID,
    string RequestNumber,
    DateOnly RequestDate,
    string InspectionPurpose,
    string RequesterName,
    string RequestingDepartment,
    string RecipientPerson,
    string? RecipientDepartment,
    DateTime InspectionDeadline,
    string Status,
    string? Priority,
    DateTime CreatedAt,
    int LinkedVoucherCount);

public record InspectionRequestDetailDto(
    int RequestID,
    string RequestNumber,
    DateOnly RequestDate,
    string InspectionPurpose,
    string RequesterName,
    string RequestingDepartment,
    string RecipientPerson,
    string? RecipientDepartment,
    DateTime InspectionDeadline,
    string Status,
    string? Priority,
    decimal? InspectionQuantity,
    string? Description,
    int? ProductionOrderId,
    int? StatisticalSheetId,
    string? InspectionSubject,
    int? SubcontractingOrderId,
    int? ProductId,
    DateTime CreatedAt,
    IReadOnlyList<LinkedVoucherSummaryDto> LinkedVouchers);

public record LinkedVoucherSummaryDto(int VoucherID, string VoucherNumber, string Status, string Conclusion);

public interface IQualityInspectionRequestRepository
{
    Task<int> AddAsync(QualityInspectionRequest request, CancellationToken ct);
    Task<QualityInspectionRequest?> GetByIdAsync(int requestId, CancellationToken ct);
    Task<bool> RequestNumberExistsAsync(string requestNumber, CancellationToken ct);
    Task<(IReadOnlyList<InspectionRequestDto> Items, int Total)> GetListAsync(
        string? status, string? purpose, DateOnly? from, DateOnly? to,
        int page, int pageSize, CancellationToken ct);
    Task<InspectionRequestDetailDto?> GetDetailAsync(int requestId, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
