namespace AeroMes.Domain.Master.Repositories;

public interface IMoldRepository
{
    /// <summary>Tracked load with product mappings and maintenance logs, for command handlers.</summary>
    Task<Mold?> GetByCodeAsync(string code, CancellationToken ct = default);

    /// <summary>No-tracking load with machine, product and maintenance details, for the detail query.</summary>
    Task<Mold?> GetByCodeWithDetailsAsync(string code, CancellationToken ct = default);

    Task<bool> CodeExistsAsync(string code, CancellationToken ct = default);

    Task<IReadOnlyList<Mold>> GetAllAsync(
        bool activeOnly = true,
        MoldStatus? status = null,
        string? machineCode = null,
        string? productCode = null,
        string? search = null,
        CancellationToken ct = default);

    /// <summary>Molds whose shots since last PM reached the given fraction of the PM interval.</summary>
    Task<IReadOnlyList<Mold>> GetDueForPmAsync(double threshold = 0.9, CancellationToken ct = default);

    Task AddAsync(Mold entity, CancellationToken ct = default);
}
