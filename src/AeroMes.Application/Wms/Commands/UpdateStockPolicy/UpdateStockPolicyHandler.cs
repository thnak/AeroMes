using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Wms.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.UpdateStockPolicy;

public class UpdateStockPolicyHandler(
    IStockPolicyRepository repo,
    IUnitOfWork uow,
    IValidator<UpdateStockPolicyCommand> validator)
    : ICommandHandler<UpdateStockPolicyCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        UpdateStockPolicyCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<Unit>.Invalid(validation.ToErrorDictionary());

        try
        {
            var policy = await repo.GetByIdAsync(cmd.PolicyId, ct);
            if (policy is null)
                return ValidationResult<Unit>.NotFound($"Không tìm thấy chính sách tồn kho #{cmd.PolicyId}.");

            policy.Update(cmd.MinQty, cmd.MaxQty, cmd.SafetyStockQty,
                cmd.ReorderQty, cmd.LeadTimeDays, cmd.IsActive, cmd.UpdatedBy);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
