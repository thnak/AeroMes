using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Routings.Commands.AddRoutingStep;

public class AddRoutingStepHandler(
    IRoutingRepository repo,
    IUnitOfWork uow,
    IValidator<AddRoutingStepCommand> validator) : ICommandHandler<AddRoutingStepCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(AddRoutingStepCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<int>.Invalid(validation.ToErrorDictionary());

        try
        {
            var step = RoutingStep.Create(
                cmd.RoutingId,
                cmd.StepNumber,
                cmd.OperationCode,
                cmd.DefaultWorkCenterId,
                cmd.StandardCycleTime,
                cmd.IsQcRequired);

            repo.AddStep(step);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<int>.Ok(step.RoutingStepID);
        }
        catch (EntityNotFoundException ex)
        {
            return ValidationResult<int>.NotFound(ex.Message);
        }
        catch (DomainException ex)
        {
            return ValidationResult<int>.Failure(ex.Message);
        }
    }
}
