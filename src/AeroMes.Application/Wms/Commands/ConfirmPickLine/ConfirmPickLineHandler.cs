using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Wms.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.ConfirmPickLine;

public class ConfirmPickLineHandler(
    IPickListRepository pickListRepo,
    IUnitOfWork uow,
    IValidator<ConfirmPickLineCommand> validator)
    : ICommandHandler<ConfirmPickLineCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        ConfirmPickLineCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<Unit>.Invalid(validation.ToErrorDictionary());

        try
        {
            var pickList = await pickListRepo.GetByIdWithLinesAsync(cmd.PickListId, ct);
            if (pickList is null)
                return ValidationResult<Unit>.NotFound($"Không tìm thấy phiếu lấy hàng #{cmd.PickListId}.");

            var line = pickList.Lines.FirstOrDefault(l => l.PickLineId == cmd.PickLineId);
            if (line is null)
                return ValidationResult<Unit>.NotFound(
                    $"Không tìm thấy dòng lấy hàng #{cmd.PickLineId}.");

            if (cmd.ScannedBinId.HasValue && line.BinId.HasValue && cmd.ScannedBinId != line.BinId)
                return ValidationResult<Unit>.Failure(
                    $"Mã thùng quét được ({cmd.ScannedBinId}) không khớp với vị trí phân bổ ({line.BinId}).");

            if (cmd.ScannedLotNumber is not null &&
                !string.Equals(cmd.ScannedLotNumber.Trim(), line.LotNumber, StringComparison.OrdinalIgnoreCase))
                return ValidationResult<Unit>.Failure(
                    $"Số lô quét được '{cmd.ScannedLotNumber}' không khớp với số lô phân bổ '{line.LotNumber}'.");

            pickList.ConfirmLine(cmd.PickLineId, cmd.ActualPickedQty);
            await uow.SaveChangesAsync(ct);

            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
