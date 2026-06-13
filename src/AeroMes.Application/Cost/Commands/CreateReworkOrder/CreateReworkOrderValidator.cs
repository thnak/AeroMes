using FluentValidation;

namespace AeroMes.Application.Cost.Commands.CreateReworkOrder;

public class CreateReworkOrderValidator : AbstractValidator<CreateReworkOrderCommand>
{
    public CreateReworkOrderValidator()
    {
        RuleFor(x => x.ReworkCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.SourceWOID).GreaterThan(0);
        RuleFor(x => x.ProductCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.ReworkQty).GreaterThan(0);
        RuleFor(x => x.CreatedBy).NotEmpty();
    }
}
