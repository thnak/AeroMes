namespace AeroMes.Domain.Quality.Repositories;

public interface INcrRepository
{
    Task<Ncr?> GetByIdAsync(int ncrId, CancellationToken ct = default);
    Task<Ncr?> GetByIdWithLinesAsync(int ncrId, CancellationToken ct = default);
    Task<IReadOnlyList<Ncr>> GetListAsync(string? status, string? productCode, CancellationToken ct = default);
    Task<bool> ExistsByOrderAsync(int inspectionOrderId, CancellationToken ct = default);
    void Add(Ncr ncr);
}
