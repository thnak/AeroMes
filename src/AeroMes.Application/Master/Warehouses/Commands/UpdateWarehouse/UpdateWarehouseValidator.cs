using FluentValidation;

namespace AeroMes.Application.Master.Warehouses.Commands.UpdateWarehouse;

public class UpdateWarehouseValidator : AbstractValidator<UpdateWarehouseCommand>
{
    public UpdateWarehouseValidator()
    {
        RuleFor(x => x.WarehouseId).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Address).MaximumLength(200).When(x => x.Address is not null);
    }
}
