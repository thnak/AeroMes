using AeroMes.Application.Common;
using AeroMes.Domain.Quality.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Quality.InspectionRequests.Queries.GetInspectionRequests;

public record GetInspectionRequestsQuery(
    string? Status,
    string? Purpose,
    DateOnly? From,
    DateOnly? To,
    int Page = 1,
    int PageSize = 20) : IQuery<PagedResult<InspectionRequestDto>>;
