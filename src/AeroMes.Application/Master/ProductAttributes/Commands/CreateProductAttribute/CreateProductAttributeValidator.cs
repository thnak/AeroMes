using AeroMes.Domain.Master.Repositories;
using FluentValidation;

namespace AeroMes.Application.Master.ProductAttributes.Commands.CreateProductAttribute;

public class CreateProductAttributeValidator : AbstractValidator<CreateProductAttributeCommand>
{
    public CreateProductAttributeValidator(IProductAttributeRepository repo)
    {
        RuleFor(x => x.Code)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .MaximumLength(30)
            .MustAsync(async (code, ct) => !await repo.CodeExistsAsync(code, ct))
            .WithMessage(x => $"Attribute code '{x.Code}' already exists.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Values)
            .Must(values => values
                .Select(v => v.Value.Trim().ToUpperInvariant())
                .Distinct()
                .Count() == values.Count)
            .WithMessage("Duplicate values are not allowed.");

        RuleForEach(x => x.Values).ChildRules(v =>
        {
            v.RuleFor(x => x.Value).NotEmpty().MaximumLength(100);
            v.RuleFor(x => x.GroupName).MaximumLength(100);
        });
    }
}
