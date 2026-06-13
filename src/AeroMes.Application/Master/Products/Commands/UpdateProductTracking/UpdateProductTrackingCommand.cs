using AeroMes.Application.Common;
using AeroMes.Domain.Master;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Products.Commands.UpdateProductTracking;

public record UpdateProductTrackingCommand(
    string ProductCode,
    TrackingMethod TrackingMethod,
    string? SecondaryUnit,
    decimal? SecondaryUnitConversionFactor,
    ProductClass ProductClass,
    string UpdatedBy) : ICommand<ValidationResult<Unit>>;
