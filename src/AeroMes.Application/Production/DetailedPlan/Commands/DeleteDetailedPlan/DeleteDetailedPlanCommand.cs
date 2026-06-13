using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.DetailedPlan.Commands.DeleteDetailedPlan;

public record DeleteDetailedPlanCommand(int DetailPlanId) : ICommand<ValidationResult<Unit>>;
