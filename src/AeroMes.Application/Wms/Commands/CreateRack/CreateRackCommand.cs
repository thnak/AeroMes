using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.CreateRack;

public record CreateRackCommand(int AisleId, string RackCode, decimal? MaxWeightKg, string? CreatedBy)
    : ICommand<ValidationResult<int>>;
