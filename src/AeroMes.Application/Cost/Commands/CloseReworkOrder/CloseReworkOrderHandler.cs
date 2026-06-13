using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Cost.Repositories;
using AeroMes.Domain.Exceptions;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Cost.Commands.CloseReworkOrder;

public class CloseReworkOrderHandler(
    IReworkOrderRepository repository,
    IUnitOfWork uow)
    : ICommandHandler<CloseReworkOrderCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(CloseReworkOrderCommand command, CancellationToken ct)
    {
        var order = await repository.GetByIdAsync(command.ReworkID, ct);
        if (order is null) return ValidationResult<Unit>.NotFound($"Lệnh tái chế #{command.ReworkID} không tìm thấy.");

        try
        {
            if (order.Status == Domain.Cost.ReworkStatus.Open)
                order.Start(command.UpdatedBy);
            order.Complete(command.ActMaterialCost, command.ActLaborCost, command.ActMachineCost, command.UpdatedBy);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (DomainException ex) { return ValidationResult<Unit>.Failure(ex.Message); }
    }
}
