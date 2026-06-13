using AeroMes.Domain.Master;
using FluentValidation;

namespace AeroMes.Application.Master.ProductionProcessSteps.Commands.CreateProcessStep;

public class CreateProcessStepValidator : AbstractValidator<CreateProcessStepCommand>
{
    public CreateProcessStepValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(500);
        When(x => x.ApplicationScope == ProcessApplicationScope.ProductGroup,
            () => RuleFor(x => x.ProductGroupIdsJson).NotEmpty()
                .WithMessage("Cần chọn ít nhất một nhóm sản phẩm."));
        When(x => x.ApplicationScope == ProcessApplicationScope.SpecificProduct,
            () => RuleFor(x => x.ProductIdsJson).NotEmpty()
                .WithMessage("Cần chọn ít nhất một sản phẩm."));
    }
}
