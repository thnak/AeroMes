using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Quality.InspectionOrders.Queries.GetInspectionOrders;

public record GetInspectionOrdersQuery(string? Status, string? WorkCenter, DateOnly? Date)
    : IQuery<IReadOnlyList<InspectionOrderListDto>>;
