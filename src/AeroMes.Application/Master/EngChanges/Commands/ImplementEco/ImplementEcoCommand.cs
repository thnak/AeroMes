using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.EngChanges.Commands.ImplementEco;

public record ImplementEcoCommand(
    string EcNumber,
    string ProductCode,
    string NewVersion,
    bool CloneFromActive,
    string? UpdatedBy) : ICommand<ImplementEcoResult>;

public record ImplementEcoResult(int BomHeaderId, string ProductCode, string Version);
