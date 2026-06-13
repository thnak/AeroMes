using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.ProductFamilies.Commands.AddVariantDimension;

public sealed class AddVariantDimensionHandler(
    IProductFamilyRepository repo,
    IValidator<AddVariantDimensionCommand> validator,
    IUnitOfWork uow) : ICommandHandler<AddVariantDimensionCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(AddVariantDimensionCommand cmd, CancellationToken ct)
    {
        var vr = await validator.ValidateAsync(cmd, ct);
        if (!vr.IsValid) return ValidationResult<int>.Invalid(vr.ToErrorDictionary());

        var family = await repo.GetWithDimensionsAsync(cmd.FamilyCode.ToUpperInvariant(), ct);
        if (family is null) return ValidationResult<int>.NotFound($"Family '{cmd.FamilyCode}' not found.");

        try
        {
            var dim = family.AddDimension(cmd.DimensionName, cmd.SortOrder, cmd.IsRequired);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<int>.Ok(dim.DimensionID);
        }
        catch (DomainException ex)
        {
            return ValidationResult<int>.Failure(ex.Message);
        }
    }
}
