using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Quality.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Quality.InspectionVouchers.Commands.AddVoucherDefect;

public class AddVoucherDefectHandler(
    IQualityInspectionVoucherRepository repository,
    IUnitOfWork uow)
    : ICommandHandler<AddVoucherDefectCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(AddVoucherDefectCommand command, CancellationToken ct)
    {
        var voucher = await repository.GetByIdAsync(command.VoucherID, ct);
        if (voucher is null) return ValidationResult<Unit>.NotFound($"Phiếu kiểm tra #{command.VoucherID} không tìm thấy.");

        try
        {
            voucher.AddDefect(command.DefectCodeId, command.DefectName, command.Quantity, command.UpdatedBy);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (DomainException ex) { return ValidationResult<Unit>.Failure(ex.Message); }
    }
}
