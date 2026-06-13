using FluentValidation;

namespace AeroMes.Application.Iot.Adapters.Commands.CreateAdapter;

public class CreateAdapterValidator : AbstractValidator<CreateAdapterCommand>
{
    public CreateAdapterValidator()
    {
        RuleFor(x => x.MachineCode).NotEmpty();
        RuleFor(x => x.AdapterType).IsInEnum();
        RuleFor(x => x.ConfigJson).NotEmpty();
    }
}
