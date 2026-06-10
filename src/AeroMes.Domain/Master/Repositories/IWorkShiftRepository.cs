namespace AeroMes.Domain.Master.Repositories;

public interface IWorkShiftRepository
{
    Task<WorkShift?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<WorkShift?> GetByIdWithBreaksAsync(int id, CancellationToken ct = default);
    Task<bool> CodeExistsAsync(string code, CancellationToken ct = default);
    Task<IReadOnlyList<WorkShift>> GetAllAsync(bool activeOnly = true, CancellationToken ct = default);
    Task AddAsync(WorkShift entity, CancellationToken ct = default);
}
