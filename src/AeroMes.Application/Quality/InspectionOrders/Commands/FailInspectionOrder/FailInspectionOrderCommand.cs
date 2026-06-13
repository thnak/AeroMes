using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Quality.InspectionOrders.Commands.FailInspectionOrder;

public record FailInspectionOrderCommand(int InspectionOrderId) : ICommand<ValidationResult<Unit>>;
