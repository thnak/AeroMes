using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.Commands.CreatePackagingOrder;

public record CreatePackagingOrderCommand(
    int WOID,
    string ProductCode,
    decimal PlannedQty,
    string? Notes = null) : ICommand<ValidationResult<int>>;
