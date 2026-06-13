using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.MasterPlan.Commands.ApproveMasterPlan;

public record ApproveMasterPlanCommand(int MasterPlanId, string ApprovedBy) : ICommand<ValidationResult<Unit>>;
