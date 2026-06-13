using AeroMes.Domain.Wms.Repositories;
using FluentValidation;

namespace AeroMes.Application.Wms.Commands.CreatePurchaseOrder;

public class CreatePurchaseOrderValidator : AbstractValidator<CreatePurchaseOrderCommand>
{
    public CreatePurchaseOrderValidator(IPurchaseOrderRepository repo)
    {
        RuleFor(x => x.PoCode)
            .NotEmpty().MaximumLength(50)
            .MustAsync(async (code, ct) => !await repo.CodeExistsAsync(code, ct))
            .WithMessage(x => $"PO code '{x.PoCode}' already exists.");

        RuleFor(x => x.SupplierCode).NotEmpty().MaximumLength(30);
        RuleFor(x => x.Lines).NotEmpty().WithMessage("At least one PO line is required.");
        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.ProductCode).NotEmpty().MaximumLength(50);
            line.RuleFor(l => l.OrderedQty).GreaterThan(0);
            line.RuleFor(l => l.UnitPrice).GreaterThan(0).When(l => l.UnitPrice.HasValue);
        });
    }
}
