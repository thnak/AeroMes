using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Wms.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.AddRmaLine;

public class AddRmaLineHandler(
    IRmaRepository rmaRepo,
    IUnitOfWork uow,
    IValidator<AddRmaLineCommand> validator)
    : ICommandHandler<AddRmaLineCommand, ValidationResult<RmaLineAddedResult>>
{
    public async Task<ValidationResult<RmaLineAddedResult>> HandleAsync(
        AddRmaLineCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<RmaLineAddedResult>.Invalid(validation.ToErrorDictionary());

        try
        {
            var rma = await rmaRepo.GetByIdWithLinesAsync(cmd.RmaId, ct);
            if (rma is null)
                return ValidationResult<RmaLineAddedResult>.NotFound(
                    $"Không tìm thấy RMA #{cmd.RmaId}.");

            var line = rma.AddLine(cmd.ProductCode, cmd.LotNumber, cmd.ReturnQty);
            await uow.SaveChangesAsync(ct);

            return ValidationResult<RmaLineAddedResult>.Ok(new(line.RmaLineId));
        }
        catch (DomainException ex)
        {
            return ValidationResult<RmaLineAddedResult>.Failure(ex.Message);
        }
    }
}
