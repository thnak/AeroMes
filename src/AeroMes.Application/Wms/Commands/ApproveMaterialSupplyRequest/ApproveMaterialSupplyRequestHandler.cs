using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Wms.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.ApproveMaterialSupplyRequest;

public class ApproveMaterialSupplyRequestHandler(
    IMaterialSupplyRequestRepository requestRepo,
    IUnitOfWork uow)
    : ICommandHandler<ApproveMaterialSupplyRequestCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        ApproveMaterialSupplyRequestCommand cmd, CancellationToken ct)
    {
        var request = await requestRepo.GetByIdAsync(cmd.RequestId, ct);
        if (request is null)
            return ValidationResult<Unit>.NotFound($"Phiếu đề nghị '{cmd.RequestId}' không tồn tại.");

        try
        {
            request.Approve(cmd.ApprovedBy);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
