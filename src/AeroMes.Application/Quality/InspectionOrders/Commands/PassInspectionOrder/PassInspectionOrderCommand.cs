using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Quality.InspectionOrders.Commands.PassInspectionOrder;

public record PassInspectionOrderCommand(int InspectionOrderId) : ICommand<ValidationResult<Unit>>;
