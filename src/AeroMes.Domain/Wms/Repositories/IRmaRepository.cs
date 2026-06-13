namespace AeroMes.Domain.Wms.Repositories;

public interface IRmaRepository
{
    Task<IReadOnlyList<ReturnMerchandiseAuthorization>> GetAllAsync(
        ReturnDirection? direction = null,
        RmaStatus? status = null,
        CancellationToken ct = default);

    Task<ReturnMerchandiseAuthorization?> GetByIdAsync(int id, CancellationToken ct = default);

    Task<ReturnMerchandiseAuthorization?> GetByIdWithLinesAsync(int id, CancellationToken ct = default);

    Task<bool> RmaCodeExistsAsync(string rmaCode, CancellationToken ct = default);

    Task AddAsync(ReturnMerchandiseAuthorization rma, CancellationToken ct = default);
}
