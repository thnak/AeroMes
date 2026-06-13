using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Iot.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Iot.StateRules.Commands.ReorderStateRules;

public class ReorderStateRulesHandler(
    IMachineStateRuleRepository repo,
    IUnitOfWork uow) : ICommandHandler<ReorderStateRulesCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(ReorderStateRulesCommand cmd, CancellationToken ct)
    {
        try
        {
            var rules = await repo.GetByMachineAsync(cmd.MachineCode, ct);
            var ruleMap = rules.ToDictionary(r => r.RuleID);

            for (int i = 0; i < cmd.OrderedRuleIds.Count; i++)
            {
                if (ruleMap.TryGetValue(cmd.OrderedRuleIds[i], out var rule))
                    rule.SetPriority(i + 1);
            }

            await uow.SaveChangesAsync(ct);
            return ValidationResult<int>.Ok(cmd.OrderedRuleIds.Count);
        }
        catch (DomainException ex)
        {
            return ValidationResult<int>.Failure(ex.Message);
        }
    }
}
