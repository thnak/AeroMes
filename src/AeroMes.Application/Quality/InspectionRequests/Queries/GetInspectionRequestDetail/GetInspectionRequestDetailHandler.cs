using AeroMes.Domain.Quality.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Quality.InspectionRequests.Queries.GetInspectionRequestDetail;

public class GetInspectionRequestDetailHandler(IQualityInspectionRequestRepository repository)
    : IQueryHandler<GetInspectionRequestDetailQuery, InspectionRequestDetailDto?>
{
    public Task<InspectionRequestDetailDto?> HandleAsync(
        GetInspectionRequestDetailQuery query, CancellationToken ct)
        => repository.GetDetailAsync(query.RequestID, ct);
}
