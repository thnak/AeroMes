using AeroMes.Application.Common;
using AeroMes.Domain.Master;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.UpdateProductPickingConfig;

public record UpdateProductPickingConfigCommand(
    string ProductCode,
    PickingStrategy PickingStrategy,
    int? MinShelfLifeDaysOnIssue,
    string? UpdatedBy) : ICommand<ValidationResult<Unit>>;
