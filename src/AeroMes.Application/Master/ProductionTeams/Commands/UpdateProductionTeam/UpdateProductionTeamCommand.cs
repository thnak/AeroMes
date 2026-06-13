using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.ProductionTeams.Commands.UpdateProductionTeam;

public record UpdateProductionTeamCommand(
    string Code,
    string Name,
    int? OrgUnitId,
    int? StandardLaborQuantity,
    decimal? ProductionRate,
    bool IsOrderBasedPlanningEnabled,
    IReadOnlyList<int> ProductGroupCategoryIds,
    bool IsActive,
    string? UpdatedBy) : ICommand<ValidationResult<Unit>>;
