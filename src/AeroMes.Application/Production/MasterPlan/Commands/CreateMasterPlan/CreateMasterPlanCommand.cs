using AeroMes.Application.Common;
using AeroMes.Domain.Production;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.MasterPlan.Commands.CreateMasterPlan;

public record MasterPlanLineInput(
    string ProductCode,
    string? ProductName,
    string? UnitOfMeasure,
    decimal QuantityRequired,
    decimal DailyCapacity,
    decimal OpeningInventory,
    MpsDistributionStrategy DistributionStrategy);

public record CreateMasterPlanCommand(
    string? PlanNumber,
    string PlanName,
    string? OrganizationalUnit,
    MpsGranularity Granularity,
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    MpsDataSource DataSource,
    decimal WorkingHoursPerDay,
    int WorkingDaysPerWeek,
    IReadOnlyList<MasterPlanLineInput> Lines,
    string? CreatedBy) : ICommand<ValidationResult<int>>;

public class CreateMasterPlanValidator : AbstractValidator<CreateMasterPlanCommand>
{
    public CreateMasterPlanValidator()
    {
        RuleFor(x => x.PlanName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.PeriodStart).NotEmpty();
        RuleFor(x => x.PeriodEnd).NotEmpty().GreaterThan(x => x.PeriodStart).WithMessage("PeriodEnd must be after PeriodStart.");
        RuleFor(x => x.WorkingHoursPerDay).InclusiveBetween(1, 24);
        RuleFor(x => x.WorkingDaysPerWeek).InclusiveBetween(1, 7);
    }
}
