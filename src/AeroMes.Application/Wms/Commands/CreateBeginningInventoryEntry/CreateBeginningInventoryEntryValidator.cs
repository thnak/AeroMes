using FluentValidation;

namespace AeroMes.Application.Wms.Commands.CreateBeginningInventoryEntry;

public class CreateBeginningInventoryEntryValidator : AbstractValidator<CreateBeginningInventoryEntryCommand>
{
    public CreateBeginningInventoryEntryValidator()
    {
        RuleFor(x => x.WarehouseId).GreaterThan(0);
        RuleFor(x => x.ProductCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.UnitOfMeasure).NotEmpty().MaximumLength(20);
        RuleFor(x => x.BeginningQuantity).GreaterThanOrEqualTo(0);
    }
}
