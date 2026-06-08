using AeroMes.Domain.Master.Repositories;
using MediatR;

namespace AeroMes.Application.Master.Operations.Queries.GetOperations;

public class GetOperationsHandler(IOperationRepository repo)
    : IRequestHandler<GetOperationsQuery, IReadOnlyList<OperationDto>>
{
    public async Task<IReadOnlyList<OperationDto>> Handle(GetOperationsQuery q, CancellationToken ct)
    {
        var items = await repo.GetAllAsync(q.ActiveOnly, ct);
        return items.Select(x => new OperationDto(
            x.OperationCode,
            x.OperationName,
            x.Description,
            x.IsActive)).ToList();
    }
}
