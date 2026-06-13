using AeroMes.Application.Common;
using AeroMes.Domain.Master;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Molds.Commands.RegisterMold;

public record RegisterMoldCommand(
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
    string? CreatedBy) : ICommand<ValidationResult<string>>;
