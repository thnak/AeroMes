using AeroMes.Domain.Master;
using FluentValidation;

namespace AeroMes.Application.Master.DisassemblyBoms.Commands.CreateDisassemblyBom;

public class CreateDisassemblyBomValidator : AbstractValidator<CreateDisassemblyBomCommand>
{
    public CreateDisassemblyBomValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.BomName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.SourceProductCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.LossRatio)
            .InclusiveBetween(0, 100)
            .WithMessage("Tỷ lệ hao hụt phải trong khoảng 0–100%.");
        RuleFor(x => x.Lines)
            .NotEmpty()
            .WithMessage("Phải có ít nhất một dòng nguyên liệu.")
            .Must(lines => lines.Any(l => l.ComponentType == DisassemblyComponentType.Main))
            .WithMessage("Phải có ít nhất một dòng thành phần chính (Main).");

        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.ComponentCode).NotEmpty();
            line.RuleFor(l => l.UoMCode).NotEmpty();
            line.RuleFor(l => l.LineNo).GreaterThan(0);
        });
    }
}
