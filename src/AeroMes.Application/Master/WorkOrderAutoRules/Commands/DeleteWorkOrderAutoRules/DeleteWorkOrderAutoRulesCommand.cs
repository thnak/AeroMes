using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Master.WorkOrderAutoRules.Commands.DeleteWorkOrderAutoRules;

public record DeleteWorkOrderAutoRulesCommand(int RuleId, string? DeletedBy = null) : ICommand<ValidationResult<Unit>>;
