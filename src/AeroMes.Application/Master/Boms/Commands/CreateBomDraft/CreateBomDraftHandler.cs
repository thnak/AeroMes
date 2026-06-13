using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Application.Master.Boms.Commands.CreateBomDraft;

public class CreateBomDraftHandler(
    IBomHeaderRepository repo,
    IUnitOfWork uow,
    IValidator<CreateBomDraftCommand> validator) : ICommandHandler<CreateBomDraftCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(CreateBomDraftCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<int>.Invalid(validation.ToErrorDictionary());

        try
        {
            var header = BomHeader.Create(
                cmd.ProductCode, cmd.Version, cmd.BomType, cmd.BaseQuantity, null, cmd.Notes, cmd.CreatedBy);

            if (cmd.CloneFromVersion is not null)
            {
                var source = await repo.GetByProductAndVersionAsync(cmd.ProductCode, cmd.CloneFromVersion, ct);
                if (source is null)
                    return ValidationResult<int>.NotFound($"{nameof(BomHeader)} '{cmd.ProductCode}/{cmd.CloneFromVersion}' không tìm thấy.");
                header.CloneLinesFrom(source);
            }

            await repo.AddAsync(header, ct);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<int>.Ok(header.BomHeaderId);
        }        catch (DomainException ex)
        {
            return ValidationResult<int>.Failure(ex.Message);
        }
    }
}
