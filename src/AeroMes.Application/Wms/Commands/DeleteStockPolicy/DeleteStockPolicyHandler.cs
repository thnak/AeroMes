using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Wms.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.DeleteStockPolicy;

public class DeleteStockPolicyHandler(
    IStockPolicyRepository repo,
    IUnitOfWork uow)
    : ICommandHandler<DeleteStockPolicyCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        DeleteStockPolicyCommand cmd, CancellationToken ct)
    {
        try
        {
            var policy = await repo.GetByIdAsync(cmd.PolicyId, ct);
            if (policy is null)
                return ValidationResult<Unit>.NotFound($"Không tìm thấy chính sách tồn kho #{cmd.PolicyId}.");

            policy.SoftDelete(cmd.DeletedBy);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
