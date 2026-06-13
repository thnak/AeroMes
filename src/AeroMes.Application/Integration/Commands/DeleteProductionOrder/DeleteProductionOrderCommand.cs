using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Integration.Commands.DeleteProductionOrder;

public record DeleteProductionOrderCommand(int Id) : ICommand<ValidationResult<Unit>>;
