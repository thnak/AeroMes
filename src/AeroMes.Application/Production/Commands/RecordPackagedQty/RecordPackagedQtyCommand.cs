using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.Commands.RecordPackagedQty;

public record RecordPackagedQtyCommand(int PackagingOrderID, decimal Qty) : ICommand<ValidationResult<Unit>>;
