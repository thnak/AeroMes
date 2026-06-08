using AeroMes.Domain.Master.Repositories;
using FluentValidation;

namespace AeroMes.Application.Master.BomItems.Commands.CreateBomItem;

public class CreateBomItemValidator : AbstractValidator<CreateBomItemCommand>
{
    public CreateBomItemValidator(IProductRepository productRepo, IBomItemRepository bomRepo)
    {
        RuleFor(x => x.ParentProductCode)
            .NotEmpty().WithMessage("Parent product code is required.")
            .MustAsync(async (code, ct) => await productRepo.ExistsAsync(code, ct))
            .WithMessage(x => $"Parent product '{x.ParentProductCode}' does not exist.");

        RuleFor(x => x.ChildProductCode)
            .NotEmpty().WithMessage("Child product code is required.")
            .MustAsync(async (code, ct) => await productRepo.ExistsAsync(code, ct))
            .WithMessage(x => $"Child product '{x.ChildProductCode}' does not exist.")
            .NotEqual(x => x.ParentProductCode, StringComparer.OrdinalIgnoreCase)
            .WithMessage("Parent and child product cannot be the same.");

        RuleFor(x => x)
            .MustAsync(async (cmd, ct) => !await bomRepo.PairExistsAsync(cmd.ParentProductCode, cmd.ChildProductCode, ct))
            .WithMessage(x => $"BOM entry '{x.ParentProductCode}' → '{x.ChildProductCode}' already exists.")
            .When(x => !string.IsNullOrWhiteSpace(x.ParentProductCode) && !string.IsNullOrWhiteSpace(x.ChildProductCode));

        RuleFor(x => x.RequiredQty)
            .GreaterThan(0).WithMessage("Required quantity must be greater than zero.");

        RuleFor(x => x.ScrapFactor)
            .GreaterThanOrEqualTo(0).WithMessage("Scrap factor cannot be negative.")
            .LessThanOrEqualTo(100).WithMessage("Scrap factor cannot exceed 100%.");
    }
}
