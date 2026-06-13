using AeroMes.Application.Common;
using AeroMes.Application.Integration.Commands.SyncSalesOrders;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Integration.Commands.SyncProductionOrders;

public record SyncProductionOrdersCommand : ICommand<ValidationResult<ErpSyncResult>>;
