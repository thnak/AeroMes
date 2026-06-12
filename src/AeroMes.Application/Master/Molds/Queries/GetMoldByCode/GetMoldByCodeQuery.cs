using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.Molds.Queries.GetMoldByCode;

public record GetMoldByCodeQuery(string Code) : IQuery<MoldDetailDto?>;

public record MoldDetailDto(
    int MoldId,
    string MoldCode,
    string MoldName,
    string MoldType,
    string? Material,
    int Cavities,
    long MaxShots,
    long CurrentShots,
    long ShotsAtLastPm,
    int PmIntervalShots,
    long ShotsSinceLastPm,
    decimal ShotUtilizationPercent,
    bool PmDue,
    bool NearingEndOfLife,
    string Status,
    string? CurrentMachineCode,
    string? CurrentMachineName,
    string? StorageLocation,
    string? Manufacturer,
    DateOnly? PurchaseDate,
    decimal? PurchaseCost,
    decimal? WeightKg,
    string? Notes,
    bool IsActive,
    IReadOnlyList<MoldProductMappingDto> ProductMappings,
    IReadOnlyList<MoldMaintenanceLogDto> MaintenanceHistory);

public record MoldProductMappingDto(
    int MappingId,
    string ProductCode,
    string? ProductName,
    bool IsDefault,
    double? CycleTimeSeconds);

public record MoldMaintenanceLogDto(
    long LogId,
    string MaintenanceType,
    long ShotsAtEvent,
    DateTime StartDate,
    DateTime? EndDate,
    string? TechnicianId,
    string? Description,
    string? PartReplaced,
    decimal? Cost,
    long? NextDueShots);
