using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Wms;
using AeroMes.Domain.Wms.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.DeleteMaterialSupplyRequest;

public class DeleteMaterialSupplyRequestHandler(
    IMaterialSupplyRequestRepository requestRepo,
    IUnitOfWork uow)
    : ICommandHandler<DeleteMaterialSupplyRequestCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        DeleteMaterialSupplyRequestCommand cmd, CancellationToken ct)
    {
        var request = await requestRepo.GetByIdAsync(cmd.RequestId, ct);
        if (request is null)
            return ValidationResult<Unit>.NotFound($"Phiếu đề nghị '{cmd.RequestId}' không tồn tại.");

        if (request.Status != MaterialSupplyRequestStatus.Draft)
            return ValidationResult<Unit>.Failure(
                $"Chỉ có thể xóa phiếu đề nghị ở trạng thái Nháp. Trạng thái hiện tại: {request.Status}.");

        try
        {
            request.SoftDelete(cmd.DeletedBy);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
