using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.ProductFamilies.Commands.AddDimensionValue;

public sealed class AddDimensionValueHandler(
    IProductFamilyRepository repo,
    IValidator<AddDimensionValueCommand> validator,
    IUnitOfWork uow) : ICommandHandler<AddDimensionValueCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(AddDimensionValueCommand cmd, CancellationToken ct)
    {
        var vr = await validator.ValidateAsync(cmd, ct);
        if (!vr.IsValid) return ValidationResult<int>.Invalid(vr.ToErrorDictionary());

        var family = await repo.GetWithDimensionsAsync(cmd.FamilyCode.ToUpperInvariant(), ct);
        if (family is null) return ValidationResult<int>.NotFound($"Family '{cmd.FamilyCode}' not found.");

        var dim = family.Dimensions.FirstOrDefault(d => d.DimensionID == cmd.DimensionId);
        if (dim is null) return ValidationResult<int>.NotFound($"Dimension #{cmd.DimensionId} not found in family '{cmd.FamilyCode}'.");

        try
        {
            var val = dim.AddValue(cmd.ValueCode, cmd.ValueLabel, cmd.SortOrder);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<int>.Ok(val.ValueID);
        }
        catch (DomainException ex)
        {
            return ValidationResult<int>.Failure(ex.Message);
        }
    }
}
