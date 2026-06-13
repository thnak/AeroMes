using AeroMes.Application.Common;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Production.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.Commands.SubmitMaterialPurchaseRequest;

public class SubmitMaterialPurchaseRequestHandler(IMaterialPurchaseRequestRepository repo)
    : ICommandHandler<SubmitMaterialPurchaseRequestCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        SubmitMaterialPurchaseRequestCommand cmd, CancellationToken ct)
    {
        var request = await repo.GetByIdAsync(cmd.RequestID, ct);
        if (request is null)
            return ValidationResult<Unit>.NotFound($"Yêu cầu mua '{cmd.RequestID}' không tồn tại.");

        try { request.Submit(null); }
        catch (DomainException ex) { return ValidationResult<Unit>.Failure(ex.Message); }

        await repo.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
