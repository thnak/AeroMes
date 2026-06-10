using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.WorkOrders.Commands.StartWorkOrder;

public record StartWorkOrderCommand(int WorkOrderId, string OperatorId, DateTime? Timestamp) : ICommand<StartWorkOrderResult>;

public record StartWorkOrderResult(int WOID, string Status, DateTime ActualStartDate);
