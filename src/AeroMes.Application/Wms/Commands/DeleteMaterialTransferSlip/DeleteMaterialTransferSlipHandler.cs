using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Wms.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.DeleteMaterialTransferSlip;

public class DeleteMaterialTransferSlipHandler(
    IMaterialTransferSlipRepository slipRepo,
    IUnitOfWork uow)
    : ICommandHandler<DeleteMaterialTransferSlipCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        DeleteMaterialTransferSlipCommand cmd, CancellationToken ct)
    {
        var slip = await slipRepo.GetByIdAsync(cmd.SlipId, ct);
        if (slip is null)
            return ValidationResult<Unit>.NotFound($"Phiếu điều chuyển '{cmd.SlipId}' không tồn tại.");

        try
        {
            slip.SoftDelete(cmd.DeletedBy);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
