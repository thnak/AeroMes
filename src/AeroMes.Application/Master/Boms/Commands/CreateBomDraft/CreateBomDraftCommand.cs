using AeroMes.Application.Common;
using AeroMes.Domain.Master;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Boms.Commands.CreateBomDraft;

public record CreateBomDraftCommand(
    string ProductCode,
    string Version,
    BomType BomType,
    decimal BaseQuantity,
    string? Notes,
    string? CloneFromVersion,
    string? CreatedBy) : ICommand<ValidationResult<int>>;
