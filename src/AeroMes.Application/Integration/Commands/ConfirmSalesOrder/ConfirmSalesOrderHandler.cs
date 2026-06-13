using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Integration.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Integration.Commands.ConfirmSalesOrder;

public class ConfirmSalesOrderHandler(
    ISalesOrderRepository repo,
    IUnitOfWork uow) : ICommandHandler<ConfirmSalesOrderCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        ConfirmSalesOrderCommand cmd, CancellationToken ct = default)
    {
        var so = await repo.GetByIdAsync(cmd.SOID, ct);
        if (so is null) return ValidationResult<Unit>.NotFound($"Sales order {cmd.SOID} not found.");

        try { so.Confirm(cmd.FacilityCode, cmd.ConfirmedBy); }
        catch (DomainException ex) { return ValidationResult<Unit>.Failure(ex.Message); }

        await uow.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
