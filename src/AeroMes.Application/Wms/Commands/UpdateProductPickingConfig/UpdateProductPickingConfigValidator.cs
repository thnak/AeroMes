using FluentValidation;

namespace AeroMes.Application.Wms.Commands.UpdateProductPickingConfig;

public class UpdateProductPickingConfigValidator : AbstractValidator<UpdateProductPickingConfigCommand>
{
    public UpdateProductPickingConfigValidator()
    {
        RuleFor(x => x.ProductCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.MinShelfLifeDaysOnIssue)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MinShelfLifeDaysOnIssue.HasValue);
    }
}
