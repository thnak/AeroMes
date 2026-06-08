using AeroMes.Domain.Master.Repositories;
using FluentValidation;

namespace AeroMes.Application.Master.BomItems.Commands.UpdateBomItem;

public class UpdateBomItemValidator : AbstractValidator<UpdateBomItemCommand>
{
    public UpdateBomItemValidator(IBomItemRepository repo)
    {
        RuleFor(x => x.BomId)
            .GreaterThan(0).WithMessage("BomItem id is required.")
            .MustAsync(async (id, ct) => await repo.GetByIdAsync(id, ct) is not null)
            .WithMessage(x => $"BomItem {x.BomId} does not exist.");

        RuleFor(x => x.RequiredQty)
            .GreaterThan(0).WithMessage("Required quantity must be greater than zero.");

        RuleFor(x => x.ScrapFactor)
            .GreaterThanOrEqualTo(0).WithMessage("Scrap factor cannot be negative.")
            .LessThanOrEqualTo(100).WithMessage("Scrap factor cannot exceed 100%.");

        RuleFor(x => x.UpdatedBy)
            .NotEmpty().WithMessage("UpdatedBy is required.");
    }
}
