using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Iot.StateRules.Queries.GetStateRules;

public record GetStateRulesQuery(string MachineCode) : IQuery<IReadOnlyList<StateRuleDto>>;
