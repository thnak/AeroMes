namespace AeroMes.Domain.Master.Repositories;

public interface IWorkCalendarRepository
{
    Task<WorkCalendar?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<WorkCalendar?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default);
    Task<bool> CodeExistsAsync(string code, CancellationToken ct = default);
    Task<IReadOnlyList<WorkCalendar>> GetAllAsync(bool activeOnly = true, CancellationToken ct = default);
    Task AddAsync(WorkCalendar entity, CancellationToken ct = default);
}
