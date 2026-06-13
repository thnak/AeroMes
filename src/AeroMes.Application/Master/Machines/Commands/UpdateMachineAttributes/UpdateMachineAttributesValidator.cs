using System.Text.Json;
using AeroMes.Application.Localization;
using AeroMes.Domain.Master;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace AeroMes.Application.Master.Machines.Commands.UpdateMachineAttributes;

public sealed class UpdateMachineAttributesValidator : AbstractValidator<UpdateMachineAttributesCommand>
{
    public UpdateMachineAttributesValidator(IStringLocalizer<SharedResources> L)
    {
        RuleFor(x => x.MachineCode).NotEmpty();
        // Validate JSON well-formedness if provided
        RuleFor(x => x.CustomAttributesJson)
            .Must(json => json is null || IsValidJson(json))
            .WithMessage(_ => L["InvalidValue"].Value);
    }

    private static bool IsValidJson(string json)
    {
        try
        {
            JsonDocument.Parse(json);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
