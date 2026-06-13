namespace AeroMes.Domain.Quality.Repositories;

public record VoucherDefectDto(int DetailID, int DefectCodeId, string DefectName, decimal Quantity);

public record QualityInspectionVoucherDto(
    int VoucherID,
    string VoucherNumber,
    string VoucherName,
    string InspectionType,
    string InspectorName,
    DateOnly InspectionDate,
    int? LinkedRequestId,
    int? ProductionOrderId,
    decimal SampleQuantity,
    decimal PassingSamples,
    decimal FailingSamples,
    string Conclusion,
    string Status,
    DateTime CreatedAt,
    IReadOnlyList<VoucherDefectDto> Defects);

public interface IQualityInspectionVoucherRepository
{
    Task<int> AddAsync(QualityInspectionVoucher voucher, CancellationToken ct);
    Task<QualityInspectionVoucher?> GetByIdAsync(int voucherId, CancellationToken ct);
    Task<(IReadOnlyList<QualityInspectionVoucherDto> Items, int Total)> GetListAsync(
        string? status,
        string? inspectionType,
        DateOnly? from,
        DateOnly? to,
        int page,
        int pageSize,
        CancellationToken ct);
}
