using FluentValidation;

namespace AeroMes.Application.Wms.Commands.CreateRma;

public class CreateRmaValidator : AbstractValidator<CreateRmaCommand>
{
    public CreateRmaValidator()
    {
        RuleFor(x => x.ReturnReason).NotEmpty().MaximumLength(500);
    }
}
