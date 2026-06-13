using FluentValidation;

namespace AeroMes.Application.Wms.Commands.UpdateBeginningInventoryEntry;

public class UpdateBeginningInventoryEntryValidator : AbstractValidator<UpdateBeginningInventoryEntryCommand>
{
    public UpdateBeginningInventoryEntryValidator()
    {
        RuleFor(x => x.EntryId).GreaterThan(0);
        RuleFor(x => x.BeginningQuantity).GreaterThanOrEqualTo(0);
    }
}
