using FluentValidation;

namespace AeroMes.Application.Master.Warehouses.Commands.CreateWarehouse;

public class CreateWarehouseValidator : AbstractValidator<CreateWarehouseCommand>
{
    public CreateWarehouseValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Address).MaximumLength(200).When(x => x.Address is not null);
    }
}
