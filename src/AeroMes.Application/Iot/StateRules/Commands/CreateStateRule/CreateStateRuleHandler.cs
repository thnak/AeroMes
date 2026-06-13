using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Iot;
using AeroMes.Domain.Iot.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Iot.StateRules.Commands.CreateStateRule;

public class CreateStateRuleHandler(
    IMachineStateRuleRepository repo,
    IUnitOfWork uow,
    IValidator<CreateStateRuleCommand> validator) : ICommandHandler<CreateStateRuleCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(CreateStateRuleCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<int>.Invalid(validation.ToErrorDictionary());

        try
        {
            var entity = MachineStateRule.Create(cmd.MachineCode, cmd.Priority, cmd.TargetState,
                cmd.SignalTagKey, cmd.Operator, cmd.ThresholdValue, cmd.Hysteresis,
                cmd.MinDurationMs, cmd.Description, cmd.CreatedBy);
            await repo.AddAsync(entity, ct);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<int>.Ok(entity.RuleID);
        }
        catch (DomainException ex)
        {
            return ValidationResult<int>.Failure(ex.Message);
        }
    }
}
