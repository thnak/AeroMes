namespace AeroMes.Domain.Master.Repositories;

public interface ISubstituteMaterialRepository
{
    Task<IReadOnlyList<SubstituteMaterial>> GetAllAsync(
        string? primaryMaterialCode, SubstituteMaterialStatus? status, CancellationToken ct = default);

    Task<IReadOnlyList<SubstituteMaterial>> GetByPrimaryMaterialAsync(
        string primaryMaterialCode, CancellationToken ct = default);

    Task<SubstituteMaterial?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<bool> CodeExistsAsync(string substituteCode, CancellationToken ct = default);
    Task<bool> PairExistsAsync(string primaryCode, string substituteCode, CancellationToken ct = default);
    Task AddAsync(SubstituteMaterial entity, CancellationToken ct = default);
}
