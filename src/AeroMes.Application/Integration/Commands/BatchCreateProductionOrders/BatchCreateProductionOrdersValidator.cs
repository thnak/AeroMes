using FluentValidation;

namespace AeroMes.Application.Integration.Commands.BatchCreateProductionOrders;

public sealed class BatchCreateProductionOrdersValidator : AbstractValidator<BatchCreateProductionOrdersCommand>
{
    public BatchCreateProductionOrdersValidator()
    {
        RuleFor(x => x.Items).NotEmpty().WithMessage("At least one item is required.");
        RuleFor(x => x.Items).Must(items => items.Count <= 200)
            .WithMessage("Cannot batch-create more than 200 orders at once.");
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.ProductCode).NotEmpty().MaximumLength(50);
            item.RuleFor(x => x.TargetQuantity).GreaterThan(0);
            item.RuleFor(x => x.Priority).InclusiveBetween((byte)1, (byte)10);
        });
    }
}
