using AeroMes.Domain.Master.Repositories;
using FluentValidation;

namespace AeroMes.Application.Master.Tools.Commands.AddToolOperation;

public class AddToolOperationValidator : AbstractValidator<AddToolOperationCommand>
{
    public AddToolOperationValidator(IOperationRepository operationRepo, IProductRepository productRepo)
    {
        RuleFor(x => x.ToolCode).NotEmpty();
        RuleFor(x => x.UsageCountPerOp).GreaterThan(0);
        RuleFor(x => x.OperationCode)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .MustAsync(operationRepo.ExistsAsync)
            .WithMessage(x => $"Công đoạn '{x.OperationCode}' không tồn tại.");
        RuleFor(x => x.ProductCode!)
            .Cascade(CascadeMode.Stop)
            .MustAsync(productRepo.IsActiveAsync)
            .When(x => x.ProductCode != null)
            .WithMessage(x => $"Sản phẩm '{x.ProductCode}' không tồn tại hoặc đã ngừng hoạt động.");
    }
}
