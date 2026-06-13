using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Iot.StateRules.Commands.DeleteStateRule;

public record DeleteStateRuleCommand(int Id, string? DeletedBy) : ICommand<ValidationResult<int>>;
