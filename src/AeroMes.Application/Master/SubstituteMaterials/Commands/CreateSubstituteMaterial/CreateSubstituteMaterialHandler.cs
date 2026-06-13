using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.SubstituteMaterials.Commands.CreateSubstituteMaterial;

public class CreateSubstituteMaterialHandler(
    ISubstituteMaterialRepository repo,
    IProductRepository _productRepo,
    IUnitOfWork uow,
    IValidator<CreateSubstituteMaterialCommand> validator)
    : ICommandHandler<CreateSubstituteMaterialCommand, ValidationResult<SubstituteMaterialCreatedResult>>
{
    public async Task<ValidationResult<SubstituteMaterialCreatedResult>> HandleAsync(
        CreateSubstituteMaterialCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<SubstituteMaterialCreatedResult>.Invalid(validation.ToErrorDictionary());

        try
        {
            var primaryPrefix = cmd.PrimaryMaterialCode.Length >= 4
                ? cmd.PrimaryMaterialCode[..4].ToUpperInvariant()
                : cmd.PrimaryMaterialCode.ToUpperInvariant().PadRight(4, 'X');

            string code;
            var attempts = 0;
            do
            {
                var suffix = Guid.NewGuid().ToString("N")[..4].ToUpperInvariant();
                code = $"SUBST-{primaryPrefix}-{suffix}";
                attempts++;
            } while (await repo.CodeExistsAsync(code, ct) && attempts < 5);

            var pairExists = await repo.PairExistsAsync(
                cmd.PrimaryMaterialCode, cmd.SubstituteMaterialCode, ct);
            if (pairExists)
                return ValidationResult<SubstituteMaterialCreatedResult>.Failure(
                    "Cặp nguyên vật liệu thay thế này đã tồn tại.");

            var entity = SubstituteMaterial.Create(
                code,
                cmd.PrimaryMaterialCode,
                cmd.SubstituteMaterialCode,
                cmd.ConversionRatio,
                cmd.Priority,
                cmd.Notes,
                cmd.EffectiveDate,
                cmd.ExpiryDate,
                cmd.CreatedBy);

            await repo.AddAsync(entity, ct);
            await uow.SaveChangesAsync(ct);

            return ValidationResult<SubstituteMaterialCreatedResult>.Ok(
                new SubstituteMaterialCreatedResult(entity.SubstituteId, entity.SubstituteCode));
        }
        catch (DomainException ex)
        {
            return ValidationResult<SubstituteMaterialCreatedResult>.Failure(ex.Message);
        }
    }
}
