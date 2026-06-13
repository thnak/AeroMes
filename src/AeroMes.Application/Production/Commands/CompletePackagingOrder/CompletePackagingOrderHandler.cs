using AeroMes.Application.Common;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Production.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.Commands.CompletePackagingOrder;

public class CompletePackagingOrderHandler(IPackagingRepository repo)
    : ICommandHandler<CompletePackagingOrderCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(CompletePackagingOrderCommand cmd, CancellationToken ct)
    {
        var order = await repo.GetOrderByIdAsync(cmd.PackagingOrderID, ct);
        if (order is null) return ValidationResult<Unit>.NotFound($"PackagingOrder '{cmd.PackagingOrderID}' not found.");

        try
        {
            order.Complete();
            await repo.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
