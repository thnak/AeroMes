using FluentValidation;

namespace AeroMes.Application.Wms.Commands.UpdateBin;

public class UpdateBinValidator : AbstractValidator<UpdateBinCommand>
{
    public UpdateBinValidator()
    {
        RuleFor(x => x.BinLevel).NotEmpty().MaximumLength(10);
        RuleFor(x => x.MaxQty).GreaterThan(0).When(x => x.MaxQty.HasValue);
    }
}
