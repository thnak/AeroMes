using AeroMes.Application.Wms.Commands.CreateFactoryWarehouseExport;
using FluentValidation;

namespace AeroMes.Application.Wms.Commands.UpdateFactoryWarehouseExport;

public class UpdateFactoryWarehouseExportValidator : AbstractValidator<UpdateFactoryWarehouseExportCommand>
{
    public UpdateFactoryWarehouseExportValidator()
    {
        RuleFor(x => x.ExportId).GreaterThan(0);
        RuleFor(x => x.ReceiverOrReceivingUnit).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Lines).NotEmpty().WithMessage("Phiếu xuất phải có ít nhất một dòng vật tư.");

        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(x => x.ProductCode).NotEmpty().MaximumLength(50);
            line.RuleFor(x => x.UnitOfMeasure).NotEmpty().MaximumLength(20);
            line.RuleFor(x => x.Quantity).GreaterThan(0);
            line.RuleFor(x => x.SourceWarehouseId).GreaterThan(0);
            line.When(x => x.SpecificationCode is not null, () =>
                line.RuleFor(x => x.SpecificationCode).MaximumLength(50));
        });
    }
}
