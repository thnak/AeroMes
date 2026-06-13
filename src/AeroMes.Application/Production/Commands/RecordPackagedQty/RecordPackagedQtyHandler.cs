using AeroMes.Application.Common;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Production.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.Commands.RecordPackagedQty;

public class RecordPackagedQtyHandler(IPackagingRepository repo)
    : ICommandHandler<RecordPackagedQtyCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(RecordPackagedQtyCommand cmd, CancellationToken ct)
    {
        if (cmd.Qty <= 0) return ValidationResult<Unit>.Invalid(new Dictionary<string, string[]>
            { ["Qty"] = ["Qty must be > 0."] });

        var order = await repo.GetOrderByIdAsync(cmd.PackagingOrderID, ct);
        if (order is null) return ValidationResult<Unit>.NotFound($"PackagingOrder '{cmd.PackagingOrderID}' not found.");

        try
        {
            if (order.Status == Domain.Production.PackagingOrderStatus.Draft)
                order.Start();
            order.RecordPackaged(cmd.Qty);
            await repo.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
