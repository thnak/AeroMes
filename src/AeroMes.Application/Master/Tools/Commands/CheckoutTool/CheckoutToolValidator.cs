using AeroMes.Domain.Master.Repositories;
using FluentValidation;

namespace AeroMes.Application.Master.Tools.Commands.CheckoutTool;

public class CheckoutToolValidator : AbstractValidator<CheckoutToolCommand>
{
    public CheckoutToolValidator(IWorkCenterRepository workCenterRepo)
    {
        RuleFor(x => x.ToolCode).NotEmpty();
        RuleFor(x => x.CheckedOutBy).NotEmpty().MaximumLength(100);
        RuleFor(x => x.WorkCenterId)
            .Cascade(CascadeMode.Stop)
            .MustAsync(workCenterRepo.ExistsAsync)
            .WithMessage(x => $"Khu vực sản xuất '{x.WorkCenterId}' không tồn tại.");
    }
}
