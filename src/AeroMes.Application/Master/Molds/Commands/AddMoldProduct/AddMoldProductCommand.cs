using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Molds.Commands.AddMoldProduct;

public record AddMoldProductCommand(
    string MoldCode,
    string ProductCode,
    bool IsDefault,
    double? CycleTimeSeconds,
    string? UpdatedBy) : ICommand<ValidationResult<int>>;
