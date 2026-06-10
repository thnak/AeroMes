namespace AeroMes.Domain.Master.Repositories;

public interface IShiftTemplateRepository
{
    Task<ShiftTemplate?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<IReadOnlyList<ShiftTemplate>> GetAllAsync(bool activeOnly = true, CancellationToken ct = default);
    Task<bool> CodeExistsAsync(string code, CancellationToken ct = default);
    Task AddAsync(ShiftTemplate entity, CancellationToken ct = default);
}
