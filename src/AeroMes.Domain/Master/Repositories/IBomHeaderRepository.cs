namespace AeroMes.Domain.Master.Repositories;

public interface IBomHeaderRepository
{
    /// <summary>Tracked load with lines, for command handlers.</summary>
    Task<BomHeader?> GetByProductAndVersionAsync(string productCode, string version, CancellationToken ct = default);

    /// <summary>No-tracking load by primary key (no lines).</summary>
    Task<BomHeader?> GetByIdAsync(int bomHeaderId, CancellationToken ct = default);

    /// <summary>Tracked load of the single Active version (with lines), for supersede-on-activate.</summary>
    Task<BomHeader?> GetActiveByProductAsync(string productCode, CancellationToken ct = default);

    /// <summary>No-tracking load with lines and component products, for detail/compare queries.</summary>
    Task<BomHeader?> GetByProductAndVersionWithDetailsAsync(string productCode, string version, CancellationToken ct = default);

    /// <summary>No-tracking load of the Active version with lines and component products.</summary>
    Task<BomHeader?> GetActiveByProductWithDetailsAsync(string productCode, CancellationToken ct = default);

    /// <summary>No-tracking Active headers (with lines) for a set of products — used by multi-level explosion.</summary>
    Task<IReadOnlyList<BomHeader>> GetActiveForProductsAsync(
        IReadOnlyCollection<string> productCodes, CancellationToken ct = default);

    /// <summary>No-tracking version history (with lines) for a product, newest first.</summary>
    Task<IReadOnlyList<BomHeader>> GetVersionsByProductAsync(string productCode, CancellationToken ct = default);

    /// <summary>Tracked load of the current default BOM for the given product + type, for singleton enforcement.</summary>
    Task<BomHeader?> GetDefaultByProductAndTypeAsync(string productCode, BomType bomType, CancellationToken ct = default);

    Task<bool> VersionExistsAsync(string productCode, string version, CancellationToken ct = default);

    Task AddAsync(BomHeader entity, CancellationToken ct = default);
}
