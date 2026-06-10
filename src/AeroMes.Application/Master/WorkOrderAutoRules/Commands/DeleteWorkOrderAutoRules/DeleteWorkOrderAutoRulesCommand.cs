using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.WorkOrderAutoRules.Commands.DeleteWorkOrderAutoRules;

public record DeleteWorkOrderAutoRulesCommand(int RuleId, string? DeletedBy = null) : ICommand;
