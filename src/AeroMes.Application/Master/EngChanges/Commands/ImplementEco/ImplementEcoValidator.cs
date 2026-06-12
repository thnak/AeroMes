using AeroMes.Domain.Master.Repositories;
using FluentValidation;

namespace AeroMes.Application.Master.EngChanges.Commands.ImplementEco;

public class ImplementEcoValidator : AbstractValidator<ImplementEcoCommand>
{
    public ImplementEcoValidator(IBomHeaderRepository bomRepo, IProductRepository productRepo)
    {
        RuleFor(x => x.EcNumber).NotEmpty();
        RuleFor(x => x.NewVersion).NotEmpty().MaximumLength(20);
        RuleFor(x => x.ProductCode)
            .NotEmpty()
            .MustAsync(productRepo.IsActiveAsync)
            .WithMessage(x => $"Sản phẩm '{x.ProductCode}' không tồn tại hoặc đã ngừng hoạt động.");
        RuleFor(x => x)
            .MustAsync(async (cmd, ct) => !await bomRepo.VersionExistsAsync(cmd.ProductCode, cmd.NewVersion, ct))
            .WithMessage(x => $"BOM phiên bản '{x.NewVersion}' đã tồn tại cho sản phẩm '{x.ProductCode}'.");
    }
}
