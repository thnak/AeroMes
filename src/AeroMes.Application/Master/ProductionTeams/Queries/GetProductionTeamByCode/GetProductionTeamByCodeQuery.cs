using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.ProductionTeams.Queries.GetProductionTeamByCode;

public record GetProductionTeamByCodeQuery(string Code) : IQuery<ProductionTeamDetailDto?>;

public record ProductionTeamDetailDto(
    string TeamCode,
    string TeamName,
    int? OrgUnitId,
    string? OrgUnitName,
    int? StandardLaborQuantity,
    decimal? ProductionRate,
    bool IsOrderBasedPlanningEnabled,
    bool IsActive,
    IReadOnlyList<TeamMemberDto> Members,
    IReadOnlyList<TeamProductGroupDto> ProductGroups);

public record TeamMemberDto(int MemberId, string EmployeeCode, string? FullName, bool IsLeader);
public record TeamProductGroupDto(int CategoryId, string CategoryCode, string CategoryName);
