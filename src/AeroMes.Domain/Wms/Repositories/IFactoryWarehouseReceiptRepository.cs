namespace AeroMes.Domain.Wms.Repositories;

public interface IFactoryWarehouseReceiptRepository
{
    Task<IReadOnlyList<FactoryWarehouseReceipt>> GetAllAsync(
        FactoryReceiptType? receiptType,
        FactoryReceiptStatus? status,
        CancellationToken ct = default);
    Task<FactoryWarehouseReceipt?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<FactoryWarehouseReceipt?> GetByIdWithLinesAsync(int id, CancellationToken ct = default);
    Task<bool> VoucherNumberExistsAsync(string voucherNumber, CancellationToken ct = default);
    Task AddAsync(FactoryWarehouseReceipt receipt, CancellationToken ct = default);
}
