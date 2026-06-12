using AeroMes.Domain.Master;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Tools.Commands.UpdateTool;

public record UpdateToolCommand(
    string Code,
    string Name,
    ToolType ToolType,
    string? Brand,
    string? Model,
    string? Specification,
    int? MaxUsageCount,
    int? PmIntervalCount,
    bool RequiresCalibration,
    int? CalibrationIntervalDays,
    string? StorageLocation,
    DateOnly? PurchaseDate,
    decimal? PurchaseCost,
    string? Notes,
    bool IsActive,
    string? UpdatedBy) : ICommand;
