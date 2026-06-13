using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.DetailedPlan.Commands.FinalizeDetailedPlan;

public record FinalizeDetailedPlanCommand(int DetailPlanId, string UpdatedBy) : ICommand<ValidationResult<Unit>>;
