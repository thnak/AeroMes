using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.Commands.CompletePackagingOrder;

public record CompletePackagingOrderCommand(int PackagingOrderID) : ICommand<ValidationResult<Unit>>;
