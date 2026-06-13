using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Iot.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Iot.StateRules.Commands.DeleteStateRule;

public class DeleteStateRuleHandler(
    IMachineStateRuleRepository repo,
    IUnitOfWork uow) : ICommandHandler<DeleteStateRuleCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(DeleteStateRuleCommand cmd, CancellationToken ct)
    {
        try
        {
            var entity = await repo.GetByIdAsync(cmd.Id, ct);
            if (entity is null)
                return ValidationResult<int>.NotFound($"Rule {cmd.Id} not found.");

            entity.SoftDelete(cmd.DeletedBy);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<int>.Ok(entity.RuleID);
        }
        catch (DomainException ex)
        {
            return ValidationResult<int>.Failure(ex.Message);
        }
    }
}
