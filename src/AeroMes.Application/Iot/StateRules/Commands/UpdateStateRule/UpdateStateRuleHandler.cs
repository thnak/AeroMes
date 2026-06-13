using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Iot.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Iot.StateRules.Commands.UpdateStateRule;

public class UpdateStateRuleHandler(
    IMachineStateRuleRepository repo,
    IUnitOfWork uow,
    IValidator<UpdateStateRuleCommand> validator) : ICommandHandler<UpdateStateRuleCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(UpdateStateRuleCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<int>.Invalid(validation.ToErrorDictionary());

        try
        {
            var entity = await repo.GetByIdAsync(cmd.Id, ct);
            if (entity is null)
                return ValidationResult<int>.NotFound($"Rule {cmd.Id} not found.");

            entity.Update(cmd.TargetState, cmd.SignalTagKey, cmd.Operator, cmd.ThresholdValue,
                cmd.Hysteresis, cmd.MinDurationMs, cmd.Description, cmd.IsActive, cmd.UpdatedBy);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<int>.Ok(entity.RuleID);
        }
        catch (DomainException ex)
        {
            return ValidationResult<int>.Failure(ex.Message);
        }
    }
}
