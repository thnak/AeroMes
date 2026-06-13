using FluentValidation;

namespace AeroMes.Application.Wms.Commands.CreateFactoryWarehouseReceipt;

public class CreateFactoryWarehouseReceiptValidator : AbstractValidator<CreateFactoryWarehouseReceiptCommand>
{
    public CreateFactoryWarehouseReceiptValidator()
    {
        RuleFor(x => x.SupplierOrTransferringUnit).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Notes).MaximumLength(500);
        RuleFor(x => x.Lines).NotEmpty().WithMessage("Phiếu nhập phải có ít nhất một dòng vật tư.");

        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.ProductCode).NotEmpty().MaximumLength(50);
            line.RuleFor(l => l.UnitOfMeasure).NotEmpty().MaximumLength(20);
            line.RuleFor(l => l.Quantity).GreaterThan(0);
            line.RuleFor(l => l.DestinationWarehouseId).GreaterThan(0);
            line.RuleFor(l => l.SpecificationCode).MaximumLength(50).When(l => l.SpecificationCode is not null);
        });
    }
}
