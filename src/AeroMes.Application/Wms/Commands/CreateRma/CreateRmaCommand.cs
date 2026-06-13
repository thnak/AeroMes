using AeroMes.Application.Common;
using AeroMes.Domain.Wms;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.CreateRma;

public record CreateRmaCommand(
    ReturnDirection ReturnDirection,
    string? SourceDocumentType,
    int? SourceDocumentId,
    string ReturnReason,
    string? CreatedBy) : ICommand<ValidationResult<RmaCreatedResult>>;

public record RmaCreatedResult(int RmaId, string RmaCode);
