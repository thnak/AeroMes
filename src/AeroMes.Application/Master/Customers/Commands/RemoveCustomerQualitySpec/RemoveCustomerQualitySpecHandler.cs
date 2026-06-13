using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Master.Customers.Commands.RemoveCustomerQualitySpec;

public class RemoveCustomerQualitySpecHandler(
    ICustomerRepository repo,
    IUnitOfWork uow) : ICommandHandler<RemoveCustomerQualitySpecCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(RemoveCustomerQualitySpecCommand cmd, CancellationToken ct)
    {
        var customer = await repo.GetByIdWithDetailsAsync(cmd.CustomerCode, ct);
        if (customer is null) return ValidationResult<Unit>.NotFound($"Entity '{cmd.CustomerCode}' was not found.");
        customer.RemoveQualitySpec(cmd.CustomerQualitySpecId);
        await uow.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
