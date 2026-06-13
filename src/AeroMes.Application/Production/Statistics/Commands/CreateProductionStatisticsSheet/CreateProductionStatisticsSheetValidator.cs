using FluentValidation;

namespace AeroMes.Application.Production.Statistics.Commands.CreateProductionStatisticsSheet;

public sealed class CreateProductionStatisticsSheetValidator
    : AbstractValidator<CreateProductionStatisticsSheetCommand>
{
    public CreateProductionStatisticsSheetValidator()
    {
        RuleFor(x => x).Must(x => x.POID.HasValue || x.MPOId.HasValue)
            .WithMessage("Phải chọn lệnh sản xuất hoặc lệnh sản xuất nhiều sản phẩm.");
        RuleFor(x => x.OutputLines).NotEmpty()
            .WithMessage("Phiếu phải có ít nhất một dòng sản lượng.");
        RuleForEach(x => x.OutputLines).ChildRules(line =>
        {
            line.RuleFor(x => x.ProductCode).NotEmpty().MaximumLength(50);
            line.RuleFor(x => x.QualifiedQty).GreaterThanOrEqualTo(0);
            line.RuleFor(x => x.DefectiveQty).GreaterThanOrEqualTo(0);
            line.RuleFor(x => x.DefectCodeId)
                .NotNull().When(x => x.DefectiveQty > 0)
                .WithMessage("Cần chọn mã lỗi khi có sản lượng NG.");
        });
        RuleForEach(x => x.MaterialLines).ChildRules(mat =>
        {
            mat.RuleFor(x => x.MaterialCode).NotEmpty().MaximumLength(50);
            mat.RuleFor(x => x.ActualUsedQty).GreaterThanOrEqualTo(0);
            mat.RuleFor(x => x.UoMCode).NotEmpty().MaximumLength(20);
        });
    }
}
