using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Customers.Commands.RemoveCustomerQualitySpec;

public class RemoveCustomerQualitySpecHandler(
    ICustomerRepository repo,
    IUnitOfWork uow) : ICommandHandler<RemoveCustomerQualitySpecCommand>
{
    public async Task HandleAsync(RemoveCustomerQualitySpecCommand cmd, CancellationToken ct)
    {
        var customer = await repo.GetByIdWithDetailsAsync(cmd.CustomerCode, ct)
            ?? throw new EntityNotFoundException(nameof(cmd.CustomerCode), cmd.CustomerCode);
        customer.RemoveQualitySpec(cmd.CustomerQualitySpecId);
        await uow.SaveChangesAsync(ct);
    }
}
