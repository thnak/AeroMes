using FluentValidation;

namespace AeroMes.Application.Wms.Commands.DisposeRmaLine;

public class DisposeRmaLineValidator : AbstractValidator<DisposeRmaLineCommand>
{
    public DisposeRmaLineValidator()
    {
        RuleFor(x => x.DispositionLocationId).GreaterThan(0);
    }
}
