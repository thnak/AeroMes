using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Wms;
using AeroMes.Domain.Wms.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.CreateRma;

public class CreateRmaHandler(
    IRmaRepository rmaRepo,
    IUnitOfWork uow,
    IValidator<CreateRmaCommand> validator)
    : ICommandHandler<CreateRmaCommand, ValidationResult<RmaCreatedResult>>
{
    public async Task<ValidationResult<RmaCreatedResult>> HandleAsync(
        CreateRmaCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<RmaCreatedResult>.Invalid(validation.ToErrorDictionary());

        try
        {
            var rmaCode = $"RMA-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..4].ToUpperInvariant()}";

            while (await rmaRepo.RmaCodeExistsAsync(rmaCode, ct))
                rmaCode = $"RMA-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..4].ToUpperInvariant()}";

            var rma = ReturnMerchandiseAuthorization.Create(
                rmaCode,
                cmd.ReturnDirection,
                cmd.SourceDocumentType,
                cmd.SourceDocumentId,
                cmd.ReturnReason,
                cmd.CreatedBy);

            await rmaRepo.AddAsync(rma, ct);
            await uow.SaveChangesAsync(ct);

            return ValidationResult<RmaCreatedResult>.Ok(new(rma.RmaId, rma.RmaCode));
        }
        catch (DomainException ex)
        {
            return ValidationResult<RmaCreatedResult>.Failure(ex.Message);
        }
    }
}
