using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Quality.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Quality.InspectionPlans.Commands.UpdateInspectionPlan;

public class UpdateInspectionPlanHandler(
    IInspectionPlanRepository repo,
    IUnitOfWork uow,
    IValidator<UpdateInspectionPlanCommand> validator) : ICommandHandler<UpdateInspectionPlanCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(UpdateInspectionPlanCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<Unit>.Invalid(validation.ToErrorDictionary());

        var plan = await repo.GetByIdAsync(cmd.PlanId, ct);
        if (plan is null)
            return ValidationResult<Unit>.NotFound($"Inspection plan {cmd.PlanId} not found.");

        try
        {
            plan.Update(cmd.Name, cmd.RoutingStepId, cmd.ProductCode,
                cmd.SamplingMethod, cmd.SampleSize, cmd.AcceptNumber, cmd.RejectNumber,
                cmd.InspectionType, cmd.Notes);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
