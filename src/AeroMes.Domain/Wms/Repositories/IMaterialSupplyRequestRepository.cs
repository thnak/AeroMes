namespace AeroMes.Domain.Wms.Repositories;

public interface IMaterialSupplyRequestRepository
{
    Task<IReadOnlyList<MaterialSupplyRequest>> GetAllAsync(
        MaterialSupplyRequestType? requestType,
        MaterialSupplyRequestStatus? status,
        CancellationToken ct = default);
    Task<MaterialSupplyRequest?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<MaterialSupplyRequest?> GetByIdWithLinesAsync(int id, CancellationToken ct = default);
    Task<bool> VoucherNumberExistsAsync(string voucherNumber, CancellationToken ct = default);
    Task AddAsync(MaterialSupplyRequest request, CancellationToken ct = default);
}
