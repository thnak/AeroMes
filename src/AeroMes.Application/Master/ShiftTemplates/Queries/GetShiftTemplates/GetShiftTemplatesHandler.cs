using AeroMes.Domain.Master.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.ShiftTemplates.Queries.GetShiftTemplates;

public class GetShiftTemplatesHandler(IShiftTemplateRepository repo)
    : IQueryHandler<GetShiftTemplatesQuery, IReadOnlyList<ShiftTemplateDto>>
{
    public async Task<IReadOnlyList<ShiftTemplateDto>> HandleAsync(GetShiftTemplatesQuery q, CancellationToken ct)
    {
        var items = await repo.GetAllAsync(q.ActiveOnly, ct);
        return items.Select(x => new ShiftTemplateDto(
            x.ShiftCode, x.ShiftName, x.StartTime, x.EndTime,
            x.IsNightShift, x.ValidDays, x.WorkCenterId, x.IsActive)).ToList();
    }
}
