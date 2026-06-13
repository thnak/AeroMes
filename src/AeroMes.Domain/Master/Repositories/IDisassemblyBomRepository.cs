namespace AeroMes.Domain.Master.Repositories;

public interface IDisassemblyBomRepository
{
    Task<IReadOnlyList<DisassemblyBom>> GetAllAsync(
        string? sourceProductCode, DisassemblyBomStatus? status, CancellationToken ct = default);

    Task<IReadOnlyList<DisassemblyBom>> GetBySourceProductAsync(
        string sourceProductCode, CancellationToken ct = default);

    Task<DisassemblyBom?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<DisassemblyBom?> GetByIdWithLinesAsync(int id, CancellationToken ct = default);
    Task<DisassemblyBom?> GetDefaultBySourceProductAsync(string sourceProductCode, CancellationToken ct = default);
    Task<bool> CodeExistsAsync(string bomCode, CancellationToken ct = default);
    Task AddAsync(DisassemblyBom entity, CancellationToken ct = default);
}
