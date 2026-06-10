using FluentValidation;

namespace AeroMes.Application.Auth.PermissionOverrides.Commands.AddPermissionOverride;

public class AddPermissionOverrideValidator : AbstractValidator<AddPermissionOverrideCommand>
{
    public AddPermissionOverrideValidator()
    {
        RuleFor(x => x.PermissionCode).NotEmpty();
        RuleFor(x => x.Effect)
            .Must(e => e.Equals("Grant", StringComparison.OrdinalIgnoreCase)
                    || e.Equals("Deny", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Effect must be 'Grant' or 'Deny'.");
    }
}
