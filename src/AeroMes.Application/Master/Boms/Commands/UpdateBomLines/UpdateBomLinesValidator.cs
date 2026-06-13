using AeroMes.Domain.Master.Repositories;
using FluentValidation;

namespace AeroMes.Application.Master.Boms.Commands.UpdateBomLines;

public class UpdateBomLinesValidator : AbstractValidator<UpdateBomLinesCommand>
{
    public UpdateBomLinesValidator(IProductRepository productRepo, IUnitOfMeasureRepository uomRepo)
    {
        RuleFor(x => x.ProductCode).NotEmpty();
        RuleFor(x => x.Version).NotEmpty();
        RuleFor(x => x.Lines)
            .Must(lines => lines.Select(l => l.LineNo).Distinct().Count() == lines.Count)
            .WithMessage("Số thứ tự dòng (LineNo) không được trùng lặp.");

        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.LineNo).GreaterThan(0);
            line.RuleFor(l => l.RequiredQty).GreaterThan(0);
            line.RuleFor(l => l.ScrapFactor).InclusiveBetween(0, 100);
            line.RuleFor(l => l.Notes).MaximumLength(200).When(l => l.Notes != null);
            line.RuleFor(l => l.ComponentCode)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .MustAsync(productRepo.ExistsAsync)
                .WithMessage(l => $"Nguyên liệu '{l.ComponentCode}' không tồn tại.");
            line.RuleFor(l => l.UoMCode)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .MustAsync(uomRepo.CodeExistsAsync)
                .WithMessage(l => $"Đơn vị tính '{l.UoMCode}' không tồn tại.");
        });
    }
}
