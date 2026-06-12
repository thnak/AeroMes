using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.ProductionTeams.Queries.GetProductionTeams;

public record GetProductionTeamsQuery(
    bool ActiveOnly,
    string? Search,
    int? OrgUnitId) : IQuery<IReadOnlyList<ProductionTeamDto>>;

public record ProductionTeamDto(
    string TeamCode,
    string TeamName,
    int? OrgUnitId,
    string? OrgUnitName,
    int? StandardLaborQuantity,
    decimal? ProductionRate,
    bool IsOrderBasedPlanningEnabled,
    bool IsActive,
    int MemberCount);
