using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Boms.Commands.UpdateBomLines;

public record UpdateBomLinesCommand(
    string ProductCode,
    string Version,
    IReadOnlyList<BomLineInput> Lines,
    string? UpdatedBy) : ICommand;

public record BomLineInput(
    int LineNo,
    string ComponentCode,
    decimal RequiredQty,
    string UoMCode,
    decimal ScrapFactor = 0m,
    bool IsPhantom = false,
    string? Notes = null);
