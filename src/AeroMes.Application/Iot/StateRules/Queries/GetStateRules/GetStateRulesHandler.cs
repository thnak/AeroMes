using AeroMes.Domain.Iot.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Iot.StateRules.Queries.GetStateRules;

public class GetStateRulesHandler(IMachineStateRuleRepository repo) : IQueryHandler<GetStateRulesQuery, IReadOnlyList<StateRuleDto>>
{
    public async Task<IReadOnlyList<StateRuleDto>> HandleAsync(GetStateRulesQuery query, CancellationToken ct)
    {
        var rules = await repo.GetByMachineAsync(query.MachineCode, ct);
        return rules.Select(r => new StateRuleDto(
            r.RuleID, r.MachineCode, r.Priority, r.TargetState, r.SignalTagKey,
            r.Operator, r.ThresholdValue, r.Hysteresis, r.MinDurationMs, r.IsActive, r.Description)).ToList();
    }
}
