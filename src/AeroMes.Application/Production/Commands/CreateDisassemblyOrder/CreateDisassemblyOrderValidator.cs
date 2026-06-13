using FluentValidation;

namespace AeroMes.Application.Production.Commands.CreateDisassemblyOrder;

public class CreateDisassemblyOrderValidator : AbstractValidator<CreateDisassemblyOrderCommand>
{
    public CreateDisassemblyOrderValidator()
    {
        RuleFor(x => x.SourceProductCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.SourceQty).GreaterThan(0);
    }
}
