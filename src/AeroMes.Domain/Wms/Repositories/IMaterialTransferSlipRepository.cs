namespace AeroMes.Domain.Wms.Repositories;

public interface IMaterialTransferSlipRepository
{
    Task<IReadOnlyList<MaterialTransferSlip>> GetAllAsync(
        MaterialTransferType? transferType,
        MaterialTransferStatus? status,
        CancellationToken ct = default);
    Task<MaterialTransferSlip?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<MaterialTransferSlip?> GetByIdWithLinesAsync(int id, CancellationToken ct = default);
    Task<bool> VoucherNumberExistsAsync(string voucherNumber, CancellationToken ct = default);
    Task AddAsync(MaterialTransferSlip slip, CancellationToken ct = default);
}
