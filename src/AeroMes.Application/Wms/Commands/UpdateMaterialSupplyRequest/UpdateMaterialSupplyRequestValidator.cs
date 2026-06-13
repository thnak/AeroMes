using AeroMes.Application.Wms.Commands.CreateMaterialSupplyRequest;
using FluentValidation;

namespace AeroMes.Application.Wms.Commands.UpdateMaterialSupplyRequest;

public class UpdateMaterialSupplyRequestValidator : AbstractValidator<UpdateMaterialSupplyRequestCommand>
{
    public UpdateMaterialSupplyRequestValidator()
    {
        RuleFor(x => x.RequestId).GreaterThan(0);
        RuleFor(x => x.RequesterUnit).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Notes).MaximumLength(500);
        RuleFor(x => x.Lines).NotEmpty().WithMessage("Phiếu đề nghị phải có ít nhất một dòng vật tư.");

        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.ProductCode).NotEmpty().MaximumLength(50);
            line.RuleFor(l => l.UnitOfMeasure).NotEmpty().MaximumLength(20);
            line.RuleFor(l => l.RequestedQuantity).GreaterThan(0);
            line.RuleFor(l => l.WarehouseId).GreaterThan(0).When(l => l.WarehouseId.HasValue);
            line.RuleFor(l => l.Notes).MaximumLength(200).When(l => l.Notes is not null);
        });
    }
}
