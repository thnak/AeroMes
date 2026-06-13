using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Integration.Commands.RejectSalesOrder;

public record RejectSalesOrderCommand(int SOID, string RejectedBy) : ICommand<ValidationResult<Unit>>;
