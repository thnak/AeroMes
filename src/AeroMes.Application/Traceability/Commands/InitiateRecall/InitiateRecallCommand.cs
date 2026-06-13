using AeroMes.Application.Common;
using AeroMes.Domain.Traceability;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Traceability.Commands.InitiateRecall;

public sealed record InitiateRecallCommand(
    string Title,
    RecallType RecallType,
    string AnchorLotNumber,
    AnchorDirection AnchorDirection,
    string InitiatedBy,
    string? Description,
    string? RegulatoryRef)
    : ICommand<ValidationResult<Guid>>;
