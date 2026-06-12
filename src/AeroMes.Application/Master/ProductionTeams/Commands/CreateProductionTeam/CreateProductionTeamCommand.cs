using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.ProductionTeams.Commands.CreateProductionTeam;

public record CreateProductionTeamCommand(
    string Code,
    string Name,
    int? OrgUnitId,
    int? StandardLaborQuantity,
    decimal? ProductionRate,
    bool IsOrderBasedPlanningEnabled,
    IReadOnlyList<int> ProductGroupCategoryIds,
    string? CreatedBy) : ICommand<string>;
