using AeroMes.Domain.Quality.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Quality.InspectionRequests.Queries.GetInspectionRequestDetail;

public record GetInspectionRequestDetailQuery(int RequestID) : IQuery<InspectionRequestDetailDto?>;
