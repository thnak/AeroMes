using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Quality.InspectionOrders.Commands.AssignInspectionOrder;

public record AssignInspectionOrderCommand(int InspectionOrderId, string InspectorCode)
    : ICommand<ValidationResult<Unit>>;
