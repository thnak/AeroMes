using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Quality.InspectionOrders.Queries.GetInspectionOrderDetail;

public record GetInspectionOrderDetailQuery(int InspectionOrderId) : IQuery<InspectionOrderDetailDto?>;
