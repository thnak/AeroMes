using AeroMes.Application.Common;
using AeroMes.Domain.Production;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.DetailedPlan.Commands.CreateDetailedPlan;

public record DppProductLineInput(
    string ProductCode,
    string? ProductName,
    string? UnitOfMeasure,
    decimal TotalRequiredQty,
    decimal DailyCapacity);

public record CreateDetailedPlanCommand(
    int MasterPlanId,
    string? PlanNumber,
    string PlanName,
    DppGranularity Granularity,
    IReadOnlyList<DppProductLineInput> ProductLines,
    string? CreatedBy) : ICommand<ValidationResult<int>>;

public class CreateDetailedPlanValidator : AbstractValidator<CreateDetailedPlanCommand>
{
    public CreateDetailedPlanValidator()
    {
        RuleFor(x => x.MasterPlanId).GreaterThan(0);
        RuleFor(x => x.PlanName).NotEmpty().MaximumLength(200);
    }
}
