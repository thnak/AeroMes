using AeroMes.Application.Common;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Production.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.Commands.ApproveMaterialPurchaseRequest;

public class ApproveMaterialPurchaseRequestHandler(IMaterialPurchaseRequestRepository repo)
    : ICommandHandler<ApproveMaterialPurchaseRequestCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        ApproveMaterialPurchaseRequestCommand cmd, CancellationToken ct)
    {
        var request = await repo.GetByIdAsync(cmd.RequestID, ct);
        if (request is null)
            return ValidationResult<Unit>.NotFound($"Yêu cầu mua '{cmd.RequestID}' không tồn tại.");

        try
        {
            if (cmd.IsApproved) request.Approve(null);
            else request.Reject(null);
        }
        catch (DomainException ex) { return ValidationResult<Unit>.Failure(ex.Message); }

        await repo.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
