using FluentValidation;

namespace AeroMes.Application.Wms.Commands.FulfillMaterialRequisition;

public class FulfillMaterialRequisitionValidator : AbstractValidator<FulfillMaterialRequisitionCommand>
{
    public FulfillMaterialRequisitionValidator()
    {
        RuleFor(x => x.RequisitionId).GreaterThan(0);
        RuleFor(x => x.IssuanceLines).NotEmpty().WithMessage("Phải cung cấp ít nhất một dòng thực xuất.");

        RuleForEach(x => x.IssuanceLines).ChildRules(line =>
        {
            line.RuleFor(l => l.LineId).GreaterThan(0);
            line.RuleFor(l => l.ActualIssuedQuantity).GreaterThanOrEqualTo(0);
        });
    }
}
