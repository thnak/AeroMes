using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.MasterPlan.Commands.DeleteMasterPlan;

public record DeleteMasterPlanCommand(int MasterPlanId) : ICommand<ValidationResult<Unit>>;
