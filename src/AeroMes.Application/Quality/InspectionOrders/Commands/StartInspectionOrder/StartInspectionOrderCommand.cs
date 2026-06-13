using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Quality.InspectionOrders.Commands.StartInspectionOrder;

public record StartInspectionOrderCommand(int InspectionOrderId) : ICommand<ValidationResult<Unit>>;
