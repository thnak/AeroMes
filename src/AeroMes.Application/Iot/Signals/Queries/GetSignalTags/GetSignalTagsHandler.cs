using AeroMes.Domain.Iot.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Iot.Signals.Queries.GetSignalTags;

public class GetSignalTagsHandler(ISignalTagRepository repo)
    : IQueryHandler<GetSignalTagsQuery, IReadOnlyList<SignalTagDto>>
{
    public async Task<IReadOnlyList<SignalTagDto>> HandleAsync(GetSignalTagsQuery query, CancellationToken ct)
    {
        var tags = await repo.GetListAsync(query.Category, query.DataType, ct);
        return tags.Select(t => new SignalTagDto(
            t.TagId, t.Key, t.DisplayName, t.Category, t.DataType,
            t.DefaultUnit, t.TypicalMin, t.TypicalMax, t.Description, t.IsSystemDefined))
            .ToList();
    }
}
