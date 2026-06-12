namespace AeroMes.Domain.Master.Repositories;

public interface ICapabilityGroupRepository
{
    Task<CapabilityGroup?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<IReadOnlyList<CapabilityGroup>> GetAllAsync(bool activeOnly = true, CancellationToken ct = default);
    Task<bool> ExistsAsync(string code, CancellationToken ct = default);
    Task AddAsync(CapabilityGroup entity, CancellationToken ct = default);
}
