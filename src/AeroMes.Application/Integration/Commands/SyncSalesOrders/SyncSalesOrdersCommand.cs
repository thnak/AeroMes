using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Integration.Commands.SyncSalesOrders;

public record SyncSalesOrdersCommand : ICommand<ValidationResult<ErpSyncResult>>;

public record ErpSyncResult(int Created, int Updated, DateTime SyncedAt);
