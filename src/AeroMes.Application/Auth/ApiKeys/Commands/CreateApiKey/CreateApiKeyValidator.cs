using AeroMes.Application.Localization;
using AeroMes.Domain.Auth;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace AeroMes.Application.Auth.ApiKeys.Commands.CreateApiKey;

public class CreateApiKeyValidator : AbstractValidator<CreateApiKeyCommand>
{
    public CreateApiKeyValidator(IStringLocalizer<SharedResources> L)
    {
        RuleFor(x => x.KeyName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.AssignedRole)
            .NotEmpty()
            .Must(r => ApiKeyRoles.All.Contains(r))
            .WithMessage(_ => L["ApiKey_InvalidRole"].Value);
        RuleFor(x => x.OwnerUserId).NotEmpty();
        RuleFor(x => x.WorkCenterId)
            .NotNull()
            .When(x => x.AssignedRole == ApiKeyRoles.Device)
            .WithMessage(_ => L["ApiKey_DeviceNeedsWorkCenter"].Value);
    }
}
