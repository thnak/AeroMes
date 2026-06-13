using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.MRP.Commands.CreateMrp;

public record CreateMrpCommand(
    string PlanNumber, string PlanName, int? MasterPlanId,
    string? OrganizationalUnit, DateOnly PeriodStart, DateOnly PeriodEnd,
    string? Notes, string? CreatedBy)
    : ICommand<ValidationResult<int>>;
