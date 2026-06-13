using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Wms.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Application.Wms.Commands.AddGrnLine;

public class AddGrnLineHandler(
    IGoodsReceiptNoteRepository grnRepo,
    IUnitOfWork uow,
    IValidator<AddGrnLineCommand> validator)
    : ICommandHandler<AddGrnLineCommand, ValidationResult<GrnLineAddedResult>>
{
    public async Task<ValidationResult<GrnLineAddedResult>> HandleAsync(AddGrnLineCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<GrnLineAddedResult>.Invalid(validation.ToErrorDictionary());

        try
        {
            var grn = await grnRepo.GetByIdWithLinesAsync(cmd.GrnId, ct);
            if (grn is null) return ValidationResult<GrnLineAddedResult>.NotFound($"GoodsReceiptNote '{cmd.GrnId}' was not found.");

            var line = grn.AddLine(
                cmd.PoLineId, cmd.ProductCode, cmd.LotNumber, cmd.ReceivedQty,
                cmd.ManufacturedDate, cmd.ExpiryDate, cmd.GrossWeightKg, cmd.DestinationBinId);

            await uow.SaveChangesAsync(ct);

            return ValidationResult<GrnLineAddedResult>.Ok(new GrnLineAddedResult(line.GrnLineId));
        }        catch (DomainException ex)
        {
            return ValidationResult<GrnLineAddedResult>.Failure(ex.Message);
        }
    }
}
