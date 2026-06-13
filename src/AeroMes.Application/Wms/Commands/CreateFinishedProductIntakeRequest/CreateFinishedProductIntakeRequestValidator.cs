using FluentValidation;

namespace AeroMes.Application.Wms.Commands.CreateFinishedProductIntakeRequest;

public class CreateFinishedProductIntakeRequestValidator
    : AbstractValidator<CreateFinishedProductIntakeRequestCommand>
{
    public CreateFinishedProductIntakeRequestValidator()
    {
        RuleFor(x => x.RequesterUnit).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ProductionOrderId).GreaterThan(0).When(x => x.ProductionOrderId.HasValue);
        RuleFor(x => x.Notes).MaximumLength(500);
        RuleFor(x => x.Lines).NotEmpty().WithMessage("Yêu cầu nhập phải có ít nhất một dòng.");

        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.ProductCode).NotEmpty().MaximumLength(50);
            line.RuleFor(l => l.UnitOfMeasure).NotEmpty().MaximumLength(20);
            line.RuleFor(l => l.RequestedQuantity).GreaterThan(0);
            line.RuleFor(l => l.WarehouseId).GreaterThan(0);
            line.RuleFor(l => l.DefectReason)
                .NotEmpty().WithMessage("Phải nhập lý do lỗi khi đánh dấu thành phẩm lỗi.")
                .MaximumLength(500)
                .When(l => l.IsDefective);
            line.RuleFor(l => l.Notes).MaximumLength(200).When(l => l.Notes is not null);
        });
    }
}
