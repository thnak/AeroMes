namespace AeroMes.Domain.Master.Repositories;

public interface IToolRepository
{
    /// <summary>Tracked load with mappings, open checkouts and maintenance logs, for command handlers.</summary>
    Task<Tool?> GetByCodeAsync(string code, CancellationToken ct = default);

    /// <summary>No-tracking load with work center, operation/product and history details, for the detail query.</summary>
    Task<Tool?> GetByCodeWithDetailsAsync(string code, CancellationToken ct = default);

    Task<bool> CodeExistsAsync(string code, CancellationToken ct = default);

    Task<IReadOnlyList<Tool>> GetAllAsync(
        bool activeOnly = true,
        ToolType? toolType = null,
        ToolStatus? status = null,
        int? workCenterId = null,
        string? search = null,
        CancellationToken ct = default);

    /// <summary>Calibrated tools whose NextCalibrationDue falls within the given number of days (or is overdue).</summary>
    Task<IReadOnlyList<Tool>> GetDueForCalibrationAsync(int withinDays = 7, CancellationToken ct = default);

    /// <summary>Tools whose usage since last reconditioning reached the given fraction of the PM interval.</summary>
    Task<IReadOnlyList<Tool>> GetDueForReconditioningAsync(double threshold = 0.9, CancellationToken ct = default);

    Task AddAsync(Tool entity, CancellationToken ct = default);
}
