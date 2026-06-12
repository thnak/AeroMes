using AeroMes.Domain.Master.Repositories;
using FluentValidation;

namespace AeroMes.Application.Master.Products.Commands.AddProductUoMConversion;

public class AddProductUoMConversionValidator : AbstractValidator<AddProductUoMConversionCommand>
{
    public AddProductUoMConversionValidator(IUnitOfMeasureRepository uomRepo)
    {
        RuleFor(x => x.UoMCode)
            .NotEmpty()
            .MaximumLength(20)
            .MustAsync(async (code, ct) => await uomRepo.CodeExistsAsync(code, ct))
            .WithMessage(x => $"UoM '{x.UoMCode}' not found.");

        RuleFor(x => x.ToBaseFactor)
            .GreaterThan(0);

        RuleFor(x => x.Notes)
            .MaximumLength(255);
    }
}
