using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Iot.StateRules.Commands.ReorderStateRules;

public record ReorderStateRulesCommand(
    string MachineCode,
    List<int> OrderedRuleIds,
    string UpdatedBy) : ICommand<ValidationResult<int>>;
