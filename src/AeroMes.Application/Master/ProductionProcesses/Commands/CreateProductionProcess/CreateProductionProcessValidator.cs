using FluentValidation;

namespace AeroMes.Application.Master.ProductionProcesses.Commands.CreateProductionProcess;

public class CreateProductionProcessValidator : AbstractValidator<CreateProductionProcessCommand>
{
    public CreateProductionProcessValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Stages).NotEmpty().WithMessage("Quy trình phải có ít nhất một công đoạn.");
        RuleFor(x => x.Stages).Must(s => s != null && s.Count(st => st.IsPrimaryStage) == 1)
            .WithMessage("Quy trình phải có đúng một công đoạn chính.");
    }
}
