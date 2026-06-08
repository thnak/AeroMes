using MediatR;

namespace AeroMes.Application.WorkOrders.Commands.StartWorkOrder;

public record StartWorkOrderCommand(
    int WorkOrderId,
    string OperatorId,
    string MachineCode,
    DateTime Timestamp
) : IRequest<StartWorkOrderResult>;

public record StartWorkOrderResult(int WorkOrderId, string Status, DateTime ActualStartDate);
