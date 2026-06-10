namespace AeroMes.Domain.Master.Repositories;

public interface IUnitOfMeasureRepository
{
    Task<UnitOfMeasure?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<bool> CodeExistsAsync(string code, CancellationToken ct = default);
    Task<IReadOnlyList<UnitOfMeasure>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(UnitOfMeasure entity, CancellationToken ct = default);
}
