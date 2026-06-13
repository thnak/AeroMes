using AeroMes.Domain.Production.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.Molds.Queries.GetMoldAssignmentHistory;

public class GetMoldAssignmentHistoryHandler(IMoldAssignmentRepository repo)
    : IQueryHandler<GetMoldAssignmentHistoryQuery, IReadOnlyList<MoldAssignmentDto>>
{
    public Task<IReadOnlyList<MoldAssignmentDto>> HandleAsync(
        GetMoldAssignmentHistoryQuery query, CancellationToken ct)
        => repo.GetHistoryAsync(query.MoldCode, query.FromDate, query.ToDate, ct);
}
