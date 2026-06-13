using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Quality.InspectionOrders.Commands.WaiveInspectionOrder;

public record WaiveInspectionOrderCommand(int InspectionOrderId, string WaivedBy, string Reason)
    : ICommand<ValidationResult<Unit>>;
