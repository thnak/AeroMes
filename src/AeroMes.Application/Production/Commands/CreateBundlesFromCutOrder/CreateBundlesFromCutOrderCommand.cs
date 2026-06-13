using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.Commands.CreateBundlesFromCutOrder;

public record CreateBundlesFromCutOrderCommand(
    int CutOrderID,
    int BundleSizePerBundle = 12) : ICommand<ValidationResult<int>>;
