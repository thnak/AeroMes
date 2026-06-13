using AeroMes.Domain.Production.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Production.Queries.GetProductionPlanGantt;

public record GetProductionPlanGanttQuery(
    DateTime From,
    DateTime To,
    string? TeamCode = null) : IQuery<IReadOnlyList<GanttTeamDto>>;
