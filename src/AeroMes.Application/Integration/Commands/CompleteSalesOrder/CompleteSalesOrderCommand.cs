using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Integration.Commands.CompleteSalesOrder;

public record CompleteSalesOrderCommand(int SOID, string CompletedBy) : ICommand<ValidationResult<Unit>>;
