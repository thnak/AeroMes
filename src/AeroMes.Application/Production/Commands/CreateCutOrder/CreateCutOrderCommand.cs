using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.Commands.CreateCutOrder;

public record CutOrderLineInput(string SizeCode, int QuantityToCut);

public record CreateCutOrderCommand(
    string CutOrderCode,
    int WOID,
    string StyleCode,
    string ColorCode,
    string FabricProductCode,
    string ShadeCode,
    int PlyCount,
    decimal SpreadLengthMeters,
    decimal FabricWidthCm,
    IReadOnlyList<CutOrderLineInput> Lines,
    string? MarkerReference = null,
    decimal? EstimatedFabricMeters = null) : ICommand<ValidationResult<int>>;
