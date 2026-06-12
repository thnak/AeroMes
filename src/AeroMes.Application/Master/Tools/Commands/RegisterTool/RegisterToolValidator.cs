using AeroMes.Domain.Master.Repositories;
using FluentValidation;

namespace AeroMes.Application.Master.Tools.Commands.RegisterTool;

public class RegisterToolValidator : AbstractValidator<RegisterToolCommand>
{
    public RegisterToolValidator(IToolRepository repo)
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.ToolType).IsInEnum();
        RuleFor(x => x.Brand).MaximumLength(100).When(x => x.Brand != null);
        RuleFor(x => x.Model).MaximumLength(100).When(x => x.Model != null);
        RuleFor(x => x.Specification).MaximumLength(300).When(x => x.Specification != null);
        RuleFor(x => x.MaxUsageCount).GreaterThan(0).When(x => x.MaxUsageCount != null);
        RuleFor(x => x.PmIntervalCount).GreaterThan(0).When(x => x.PmIntervalCount != null);
        RuleFor(x => x.CalibrationIntervalDays).GreaterThan(0).When(x => x.CalibrationIntervalDays != null);
        RuleFor(x => x.CalibrationIntervalDays)
            .NotNull()
            .When(x => x.RequiresCalibration)
            .WithMessage("Dụng cụ yêu cầu hiệu chuẩn phải có chu kỳ hiệu chuẩn (ngày).");
        RuleFor(x => x.StorageLocation).MaximumLength(100).When(x => x.StorageLocation != null);
        RuleFor(x => x.PurchaseCost).GreaterThanOrEqualTo(0).When(x => x.PurchaseCost != null);
        RuleFor(x => x.Notes).MaximumLength(500).When(x => x.Notes != null);
        RuleFor(x => x.Code)
            .MustAsync(async (code, ct) => !await repo.CodeExistsAsync(code, ct))
            .WithMessage(x => $"Mã dụng cụ '{x.Code}' đã tồn tại.");
    }
}
