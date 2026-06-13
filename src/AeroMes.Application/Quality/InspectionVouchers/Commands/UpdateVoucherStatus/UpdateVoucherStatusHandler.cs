using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Quality.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Quality.InspectionVouchers.Commands.UpdateVoucherStatus;

public class UpdateVoucherStatusHandler(
    IQualityInspectionVoucherRepository repository,
    IUnitOfWork uow)
    : ICommandHandler<UpdateVoucherStatusCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(UpdateVoucherStatusCommand command, CancellationToken ct)
    {
        var voucher = await repository.GetByIdAsync(command.VoucherID, ct);
        if (voucher is null) return ValidationResult<Unit>.NotFound($"Phiếu kiểm tra #{command.VoucherID} không tìm thấy.");

        try
        {
            switch (command.Action.ToLowerInvariant())
            {
                case "start":
                    voucher.Start(command.UpdatedBy);
                    break;
                case "complete":
                    if (command.Conclusion.HasValue)
                        voucher.SetConclusion(command.Conclusion.Value, command.UpdatedBy);
                    voucher.Complete(command.UpdatedBy);
                    break;
                default:
                    return ValidationResult<Unit>.Failure($"Hành động '{command.Action}' không hợp lệ. Dùng: start, complete.");
            }

            await uow.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (DomainException ex) { return ValidationResult<Unit>.Failure(ex.Message); }
    }
}
