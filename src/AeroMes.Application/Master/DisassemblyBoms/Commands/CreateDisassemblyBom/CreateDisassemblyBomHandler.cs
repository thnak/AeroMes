using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.DisassemblyBoms.Commands.CreateDisassemblyBom;

public class CreateDisassemblyBomHandler(
    IDisassemblyBomRepository repo,
    IProductRepository productRepo,
    IUnitOfWork uow,
    IValidator<CreateDisassemblyBomCommand> validator)
    : ICommandHandler<CreateDisassemblyBomCommand, ValidationResult<DisassemblyBomCreatedResult>>
{
    public async Task<ValidationResult<DisassemblyBomCreatedResult>> HandleAsync(
        CreateDisassemblyBomCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<DisassemblyBomCreatedResult>.Invalid(validation.ToErrorDictionary());

        try
        {
            var product = await productRepo.GetByCodeAsync(cmd.SourceProductCode, ct);
            if (product is null)
                return ValidationResult<DisassemblyBomCreatedResult>.NotFound(
                    $"Sản phẩm '{cmd.SourceProductCode}' không tìm thấy.");

            var prefix = cmd.SourceProductCode.Length >= 4
                ? cmd.SourceProductCode[..4]
                : cmd.SourceProductCode;

            string bomCode;
            do
            {
                bomCode = $"DBOM-{prefix}-{Guid.NewGuid().ToString("N")[..4].ToUpperInvariant()}";
            }
            while (await repo.CodeExistsAsync(bomCode, ct));

            var entity = DisassemblyBom.Create(
                bomCode, cmd.BomName, cmd.SourceProductCode,
                cmd.BomType, cmd.LossRatio,
                cmd.EffectiveDate, cmd.ExpiryDate, cmd.CreatedBy);

            entity.ReplaceLines(
                cmd.Lines.Select(l => (l.LineNo, l.ComponentCode, l.ComponentType,
                    l.RecoveryRate, l.FixedQuantity, l.UoMCode, l.Notes)).ToList(),
                cmd.CreatedBy);

            await repo.AddAsync(entity, ct);
            await uow.SaveChangesAsync(ct);

            return ValidationResult<DisassemblyBomCreatedResult>.Ok(
                new DisassemblyBomCreatedResult(entity.DisassemblyBomId, entity.BomCode));
        }
        catch (DomainException ex)
        {
            return ValidationResult<DisassemblyBomCreatedResult>.Failure(ex.Message);
        }
    }
}
