using AeroMes.Application.Common;
using AeroMes.Domain.Traceability.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Traceability.Queries.ListRecalls;

public sealed record ListRecallsQuery(string? Status, int Page = 1, int PageSize = 20)
    : IQuery<PagedResult<RecallSummaryDto>>;
