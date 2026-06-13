using FluentValidation;

namespace AeroMes.Application.Iot.Adapters.Commands.UpdateAdapter;

public class UpdateAdapterValidator : AbstractValidator<UpdateAdapterCommand>
{
    public UpdateAdapterValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.ConfigJson).NotEmpty();
    }
}
