using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.Tools.Queries.GetToolByCode;

public record GetToolByCodeQuery(string Code) : IQuery<ToolDetailDto?>;

public record ToolDetailDto(
    int ToolId,
    string ToolCode,
    string ToolName,
    string ToolType,
    string? Brand,
    string? Model,
    string? Specification,
    int? MaxUsageCount,
    int CurrentUsageCount,
    int UsageCountAtLastPm,
    int? PmIntervalCount,
    int UsageSinceLastPm,
    decimal? UsagePercent,
    bool ReconditioningDue,
    bool NearingEndOfLife,
    bool RequiresCalibration,
    int? CalibrationIntervalDays,
    DateTime? LastCalibratedAt,
    DateOnly? NextCalibrationDue,
    string Status,
    int? CurrentWorkCenterId,
    string? CurrentWorkCenterName,
    string? StorageLocation,
    DateOnly? PurchaseDate,
    decimal? PurchaseCost,
    string? Notes,
    bool IsActive,
    IReadOnlyList<ToolOperationMappingDto> OperationMappings,
    IReadOnlyList<ToolCheckoutDto> CheckoutHistory,
    IReadOnlyList<ToolMaintenanceLogDto> MaintenanceHistory);

public record ToolOperationMappingDto(
    int MappingId,
    string OperationCode,
    string? OperationName,
    string? ProductCode,
    string? ProductName,
    bool IsRequired,
    decimal UsageCountPerOp);

public record ToolCheckoutDto(
    long CheckoutId,
    int WorkCenterId,
    string? WorkCenterName,
    string CheckedOutBy,
    DateTime CheckedOutAt,
    DateTime? ExpectedReturnAt,
    DateTime? ReturnedAt,
    string? ReturnedBy,
    string? ConditionOnReturn,
    string? Notes);

public record ToolMaintenanceLogDto(
    long LogId,
    string MaintenanceType,
    int UsageAtEvent,
    DateTime PerformedAt,
    string? PerformedBy,
    decimal? Cost,
    int? NextDueCount,
    DateOnly? NextDueDate,
    string? Notes);
