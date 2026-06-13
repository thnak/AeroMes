using FluentValidation;

namespace AeroMes.Application.Wms.Commands.CreateBin;

public class CreateBinValidator : AbstractValidator<CreateBinCommand>
{
    public CreateBinValidator()
    {
        RuleFor(x => x.RackId).GreaterThan(0);
        RuleFor(x => x.BinCode).NotEmpty().MaximumLength(20);
        RuleFor(x => x.BinLevel).NotEmpty().MaximumLength(10);
        RuleFor(x => x.MaxQty).GreaterThan(0).When(x => x.MaxQty.HasValue);
    }
}
