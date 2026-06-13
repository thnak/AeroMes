using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.AcknowledgeReplenishmentAlert;

public record AcknowledgeReplenishmentAlertCommand(long AlertId, string? AcknowledgedBy)
    : ICommand<ValidationResult<Unit>>;
