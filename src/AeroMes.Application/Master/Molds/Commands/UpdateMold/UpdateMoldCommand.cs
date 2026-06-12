using AeroMes.Domain.Master;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Molds.Commands.UpdateMold;

public record UpdateMoldCommand(
    string Code,
    string Name,
    MoldType MoldType,
    string? Material,
    int Cavities,
    long MaxShots,
    int PmIntervalShots,
    string? Manufacturer,
    DateOnly? PurchaseDate,
    decimal? PurchaseCost,
    decimal? WeightKg,
    string? StorageLocation,
    string? Notes,
    bool IsActive,
    string? UpdatedBy) : ICommand;
