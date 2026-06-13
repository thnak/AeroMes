namespace AeroMes.Domain.Wms.Repositories;

public interface IMaterialRequisitionRepository
{
    Task<IReadOnlyList<MaterialRequisition>> GetAllAsync(
        int? productionOrderId,
        MaterialRequisitionStatus? status,
        CancellationToken ct = default);
    Task<MaterialRequisition?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<MaterialRequisition?> GetByIdWithLinesAsync(int id, CancellationToken ct = default);
    Task<bool> RequisitionNumberExistsAsync(string number, CancellationToken ct = default);
    Task AddAsync(MaterialRequisition requisition, CancellationToken ct = default);
}
