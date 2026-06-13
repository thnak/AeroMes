using FluentValidation;

namespace AeroMes.Application.Integration.Commands.CreateMultiProductionOrder;

public sealed class CreateMultiProductionOrderValidator : AbstractValidator<CreateMultiProductionOrderCommand>
{
    public CreateMultiProductionOrderValidator()
    {
        RuleFor(x => x.Lines).NotEmpty().WithMessage("Lệnh sản xuất phải có ít nhất một sản phẩm.");
        RuleFor(x => x.Lines).Must(l => l.Count <= 100)
            .WithMessage("Không thể tạo lệnh với hơn 100 sản phẩm.");
        RuleFor(x => x.Priority).InclusiveBetween((byte)1, (byte)10);
        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(x => x.ProductCode).NotEmpty().MaximumLength(50);
            line.RuleFor(x => x.PlannedQty).GreaterThan(0);
            line.RuleFor(x => x.UoMCode).NotEmpty().MaximumLength(20);
        });
    }
}
