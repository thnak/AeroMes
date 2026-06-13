using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Quality;
using AeroMes.Domain.Quality.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Quality.InspectionPlans.Commands.CreateInspectionPlan;

public class CreateInspectionPlanHandler(
    IInspectionPlanRepository repo,
    IUnitOfWork uow,
    IValidator<CreateInspectionPlanCommand> validator) : ICommandHandler<CreateInspectionPlanCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(CreateInspectionPlanCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<int>.Invalid(validation.ToErrorDictionary());

        try
        {
            var plan = InspectionPlan.Create(
                cmd.Code, cmd.Name, cmd.RoutingStepId, cmd.ProductCode,
                cmd.SamplingMethod, cmd.SampleSize, cmd.AcceptNumber, cmd.RejectNumber,
                cmd.InspectionType, cmd.Notes, cmd.CreatedBy);

            repo.Add(plan);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<int>.Ok(plan.PlanId);
        }
        catch (DomainException ex)
        {
            return ValidationResult<int>.Failure(ex.Message);
        }
    }
}
