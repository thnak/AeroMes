using FluentValidation;

namespace AeroMes.Application.Wms.Commands.CreateMaterialRequisition;

public class CreateMaterialRequisitionValidator : AbstractValidator<CreateMaterialRequisitionCommand>
{
    public CreateMaterialRequisitionValidator()
    {
        RuleFor(x => x.RequesterUnit).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ProductionOrderId).GreaterThan(0).When(x => x.ProductionOrderId.HasValue);
        RuleFor(x => x.Notes).MaximumLength(500);
        RuleFor(x => x.Lines).NotEmpty().WithMessage("Yêu cầu xuất phải có ít nhất một dòng vật tư.");

        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.ProductCode).NotEmpty().MaximumLength(50);
            line.RuleFor(l => l.UnitOfMeasure).NotEmpty().MaximumLength(20);
            line.RuleFor(l => l.RequestedQuantity).GreaterThan(0);
            line.RuleFor(l => l.WarehouseId).GreaterThan(0);
            line.RuleFor(l => l.Notes).MaximumLength(200).When(l => l.Notes is not null);
        });
    }
}
