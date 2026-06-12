using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Boms.Commands.CreateBomDraft;

public record CreateBomDraftCommand(
    string ProductCode,
    string Version,
    decimal BaseQuantity,
    string? Notes,
    string? CloneFromVersion,
    string? CreatedBy) : ICommand<int>;
