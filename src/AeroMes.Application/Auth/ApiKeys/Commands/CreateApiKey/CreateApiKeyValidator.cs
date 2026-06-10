using AeroMes.Domain.Auth;
using FluentValidation;

namespace AeroMes.Application.Auth.ApiKeys.Commands.CreateApiKey;

public class CreateApiKeyValidator : AbstractValidator<CreateApiKeyCommand>
{
    public CreateApiKeyValidator()
    {
        RuleFor(x => x.KeyName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.AssignedRole)
            .NotEmpty()
            .Must(r => ApiKeyRoles.All.Contains(r))
            .WithMessage($"AssignedRole must be one of: {string.Join(", ", ApiKeyRoles.All)}.");
        RuleFor(x => x.OwnerUserId).NotEmpty();
        RuleFor(x => x.WorkCenterId)
            .NotNull()
            .When(x => x.AssignedRole == ApiKeyRoles.Device)
            .WithMessage("DEVICE keys must specify a WorkCenterId.");
    }
}
