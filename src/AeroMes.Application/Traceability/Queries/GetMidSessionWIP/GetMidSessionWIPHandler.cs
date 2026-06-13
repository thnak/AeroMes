using AeroMes.Domain.Traceability.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Traceability.Queries.GetMidSessionWIP;

public sealed class GetMidSessionWIPHandler(IProcessRecordRepository repository)
    : IQueryHandler<GetMidSessionWIPQuery, IReadOnlyList<ProcessRecordDto>>
{
    public Task<IReadOnlyList<ProcessRecordDto>> HandleAsync(
        GetMidSessionWIPQuery query, CancellationToken ct)
        => repository.GetMidSessionWIPAsync(query.WorkOrderID, query.MachineCode, ct);
}
