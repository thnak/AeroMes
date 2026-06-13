using AeroMes.Domain.Master.Repositories;
using FluentValidation;

namespace AeroMes.Application.Master.Molds.Commands.AddMoldProduct;

public class AddMoldProductValidator : AbstractValidator<AddMoldProductCommand>
{
    public AddMoldProductValidator(IProductRepository productRepo)
    {
        RuleFor(x => x.MoldCode).NotEmpty();
        RuleFor(x => x.CycleTimeSeconds).GreaterThan(0).When(x => x.CycleTimeSeconds != null);
        RuleFor(x => x.ProductCode)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .MustAsync(productRepo.IsActiveAsync)
            .WithMessage(x => $"Sản phẩm '{x.ProductCode}' không tồn tại hoặc đã ngừng hoạt động.");
    }
}
