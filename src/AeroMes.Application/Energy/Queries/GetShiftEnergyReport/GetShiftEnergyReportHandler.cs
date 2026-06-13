using AeroMes.Domain.Energy.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Energy.Queries.GetShiftEnergyReport;

public class GetShiftEnergyReportHandler(IEnergyRepository repository)
    : IQueryHandler<GetShiftEnergyReportQuery, IReadOnlyList<ShiftEnergyDto>>
{
    public Task<IReadOnlyList<ShiftEnergyDto>> HandleAsync(GetShiftEnergyReportQuery query, CancellationToken ct)
        => repository.GetShiftReportAsync(query.MachineCode, query.From, query.To, ct);
}
