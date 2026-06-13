using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Quality.Ncr.Commands.CreateManualNcr;

public record CreateManualNcrCommand(
    long WorkOrderId,
    string ProductCode,
    string? LotNumber,
    decimal QtyAffected,
    string Severity,
    string CreatedBy)
    : ICommand<ValidationResult<int>>;
