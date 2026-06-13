using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Boms.Commands.UpdateBomByProducts;

public class UpdateBomByProductsHandler(
    IBomHeaderRepository repo,
    IUnitOfWork uow,
    IValidator<UpdateBomByProductsCommand> validator)
    : ICommandHandler<UpdateBomByProductsCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(UpdateBomByProductsCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<Unit>.Invalid(validation.ToErrorDictionary());

        try
        {
            var header = await repo.GetByProductAndVersionAsync(cmd.ProductCode, cmd.Version, ct);
            if (header is null)
                return ValidationResult<Unit>.NotFound($"BOM '{cmd.ProductCode}/{cmd.Version}' không tìm thấy.");

            header.ReplaceByProducts(
                cmd.ByProducts.Select(b => (b.ByProductCode, b.Quantity, b.UoMCode, b.Notes)).ToList(),
                cmd.UpdatedBy);

            await uow.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
