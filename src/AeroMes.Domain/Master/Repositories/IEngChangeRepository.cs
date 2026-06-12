namespace AeroMes.Domain.Master.Repositories;

public interface IEngChangeRepository
{
    /// <summary>Tracked load, for command handlers.</summary>
    Task<EngChange?> GetByNumberAsync(string ecNumber, CancellationToken ct = default);

    Task<bool> NumberExistsAsync(string ecNumber, CancellationToken ct = default);

    Task<IReadOnlyList<EngChange>> GetAllAsync(
        EcStatus? status = null,
        EcType? ecType = null,
        string? search = null,
        CancellationToken ct = default);

    Task AddAsync(EngChange entity, CancellationToken ct = default);
}
