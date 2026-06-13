using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.ProductFamilies.Commands.CreateProductFamily;

public sealed class CreateProductFamilyHandler(
    IProductFamilyRepository repo,
    IProductRepository products,
    IValidator<CreateProductFamilyCommand> validator,
    IUnitOfWork uow) : ICommandHandler<CreateProductFamilyCommand, ValidationResult<string>>
{
    public async Task<ValidationResult<string>> HandleAsync(CreateProductFamilyCommand cmd, CancellationToken ct)
    {
        var vr = await validator.ValidateAsync(cmd, ct);
        if (!vr.IsValid) return ValidationResult<string>.Invalid(vr.ToErrorDictionary());

        if (await repo.ExistsAsync(cmd.FamilyCode.ToUpperInvariant(), ct))
            return ValidationResult<string>.Invalid(new Dictionary<string, string[]>
                { ["FamilyCode"] = [$"Family '{cmd.FamilyCode}' already exists."] });

        var baseProduct = await products.GetByCodeAsync(cmd.BaseProductCode.ToUpperInvariant(), ct);
        if (baseProduct is null)
            return ValidationResult<string>.NotFound($"Base product '{cmd.BaseProductCode}' not found.");

        try
        {
            var family = ProductFamily.Create(cmd.FamilyCode, cmd.FamilyName, cmd.BaseProductCode, cmd.Industry);
            await repo.AddAsync(family, ct);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<string>.Ok(family.FamilyCode);
        }
        catch (DomainException ex)
        {
            return ValidationResult<string>.Failure(ex.Message);
        }
    }
}
