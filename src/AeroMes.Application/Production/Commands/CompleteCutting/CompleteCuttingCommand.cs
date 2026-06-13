using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.Commands.CompleteCutting;

public record CompleteCuttingLineInput(string SizeCode, int QuantityCut);

public record CompleteCuttingCommand(
    int CutOrderID,
    decimal ActualFabricMeters,
    decimal MarkerEfficiencyPct,
    IReadOnlyList<CompleteCuttingLineInput> Lines,
    int BundleSize = 12) : ICommand<ValidationResult<Unit>>;
