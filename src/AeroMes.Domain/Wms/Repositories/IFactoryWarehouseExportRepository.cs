namespace AeroMes.Domain.Wms.Repositories;

public interface IFactoryWarehouseExportRepository
{
    Task<IReadOnlyList<FactoryWarehouseExport>> GetAllAsync(
        FactoryExportType? exportType,
        FactoryExportStatus? status,
        CancellationToken ct = default);
    Task<FactoryWarehouseExport?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<FactoryWarehouseExport?> GetByIdWithLinesAsync(int id, CancellationToken ct = default);
    Task<bool> VoucherNumberExistsAsync(string voucherNumber, CancellationToken ct = default);
    Task AddAsync(FactoryWarehouseExport export, CancellationToken ct = default);
}
