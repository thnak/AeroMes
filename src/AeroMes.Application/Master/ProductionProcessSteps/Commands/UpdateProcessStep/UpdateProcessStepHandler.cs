using AeroMes.Application.Common;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.ProductionProcessSteps.Commands.UpdateProcessStep;

public class UpdateProcessStepHandler(IProductionProcessStepRepository repository)
    : ICommandHandler<UpdateProcessStepCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        UpdateProcessStepCommand command, CancellationToken ct)
    {
        var step = await repository.GetByIdAsync(command.StepID, ct);
        if (step is null) return ValidationResult<Unit>.NotFound($"Công đoạn #{command.StepID} không tồn tại.");

        try
        {
            step.Update(command.Name, command.Description,
                command.ApplicationScope, command.ProductGroupIdsJson,
                command.ProductIdsJson, command.UpdatedBy);
            await repository.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
