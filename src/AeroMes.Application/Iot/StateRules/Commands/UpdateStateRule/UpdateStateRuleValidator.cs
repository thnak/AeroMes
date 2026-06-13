using FluentValidation;

namespace AeroMes.Application.Iot.StateRules.Commands.UpdateStateRule;

public class UpdateStateRuleValidator : AbstractValidator<UpdateStateRuleCommand>
{
    private static readonly string[] ValidTargetStates = ["Running", "Down", "Idle", "Offline"];
    private static readonly string[] ValidOperators = ["Gt", "Lt", "Gte", "Lte", "Eq", "Neq", "Change"];

    public UpdateStateRuleValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.TargetState).Must(v => ValidTargetStates.Contains(v))
            .WithMessage($"TargetState must be one of: {string.Join(", ", ValidTargetStates)}.");
        RuleFor(x => x.Operator).Must(v => ValidOperators.Contains(v))
            .WithMessage($"Operator must be one of: {string.Join(", ", ValidOperators)}.");
    }
}
