using AeroMes.Domain.Master.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.Operations.Queries.GetOperations;

public class GetOperationsHandler(IOperationRepository repo)
    : IQueryHandler<GetOperationsQuery, IReadOnlyList<OperationDto>>
{
    public async Task<IReadOnlyList<OperationDto>> HandleAsync(GetOperationsQuery q, CancellationToken ct)
    {
        var items = await repo.GetAllAsync(q.ActiveOnly, ct);
        return items.Select(x => new OperationDto(
            x.OperationCode,
            x.OperationName,
            x.Description,
            x.IsActive)).ToList();
    }
}
