using FluentValidation;

namespace AeroMes.Application.Wms.Commands.UpdateMaterialTransferSlip;

public class UpdateMaterialTransferSlipValidator : AbstractValidator<UpdateMaterialTransferSlipCommand>
{
    public UpdateMaterialTransferSlipValidator()
    {
        RuleFor(x => x.SlipId).GreaterThan(0);
        RuleFor(x => x.SourceWarehouseId).GreaterThan(0);
        RuleFor(x => x.DestinationWarehouseId).GreaterThan(0);
        RuleFor(x => x)
            .Must(x => x.SourceWarehouseId != x.DestinationWarehouseId)
            .WithMessage("Kho nguồn và kho đích không được trùng nhau.")
            .When(x => x.SourceWarehouseId > 0 && x.DestinationWarehouseId > 0);
        RuleFor(x => x.Lines).NotEmpty().WithMessage("Phiếu điều chuyển phải có ít nhất một dòng vật tư.");

        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(x => x.ProductCode).NotEmpty().MaximumLength(50);
            line.RuleFor(x => x.UnitOfMeasure).NotEmpty().MaximumLength(20);
            line.RuleFor(x => x.Quantity).GreaterThan(0);
            line.When(x => x.SpecificationCode is not null, () =>
                line.RuleFor(x => x.SpecificationCode).MaximumLength(50));
        });
    }
}
