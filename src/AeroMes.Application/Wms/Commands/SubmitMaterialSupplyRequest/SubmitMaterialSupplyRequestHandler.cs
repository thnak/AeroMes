using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Wms.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.SubmitMaterialSupplyRequest;

public class SubmitMaterialSupplyRequestHandler(
    IMaterialSupplyRequestRepository requestRepo,
    IUnitOfWork uow)
    : ICommandHandler<SubmitMaterialSupplyRequestCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        SubmitMaterialSupplyRequestCommand cmd, CancellationToken ct)
    {
        var request = await requestRepo.GetByIdWithLinesAsync(cmd.RequestId, ct);
        if (request is null)
            return ValidationResult<Unit>.NotFound($"Phiếu đề nghị '{cmd.RequestId}' không tồn tại.");

        try
        {
            request.Submit(cmd.SubmittedBy);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
