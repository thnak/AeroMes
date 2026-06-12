using AeroMes.Domain.Master.Repositories;
using FluentValidation;

namespace AeroMes.Application.Master.Boms.Commands.CreateBomDraft;

public class CreateBomDraftValidator : AbstractValidator<CreateBomDraftCommand>
{
    public CreateBomDraftValidator(IBomHeaderRepository repo, IProductRepository productRepo)
    {
        RuleFor(x => x.Version).NotEmpty().MaximumLength(20);
        RuleFor(x => x.BaseQuantity).GreaterThan(0);
        RuleFor(x => x.Notes).MaximumLength(500).When(x => x.Notes != null);
        RuleFor(x => x.ProductCode)
            .NotEmpty()
            .MustAsync(productRepo.IsActiveAsync)
            .WithMessage(x => $"Sản phẩm '{x.ProductCode}' không tồn tại hoặc đã ngừng hoạt động.");
        RuleFor(x => x)
            .MustAsync(async (cmd, ct) => !await repo.VersionExistsAsync(cmd.ProductCode, cmd.Version, ct))
            .WithMessage(x => $"BOM phiên bản '{x.Version}' đã tồn tại cho sản phẩm '{x.ProductCode}'.");
    }
}
