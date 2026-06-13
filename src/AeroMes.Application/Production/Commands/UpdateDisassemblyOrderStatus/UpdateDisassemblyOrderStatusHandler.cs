using AeroMes.Application.Common;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Production;
using AeroMes.Domain.Production.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.Commands.UpdateDisassemblyOrderStatus;

public class UpdateDisassemblyOrderStatusHandler(IDisassemblyOrderRepository repo)
    : ICommandHandler<UpdateDisassemblyOrderStatusCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        UpdateDisassemblyOrderStatusCommand cmd, CancellationToken ct)
    {
        var order = await repo.GetByIdAsync(cmd.DisassemblyOrderID, ct);
        if (order is null) return ValidationResult<Unit>.NotFound($"DisassemblyOrder '{cmd.DisassemblyOrderID}' not found.");

        try
        {
            switch (cmd.NewStatus)
            {
                case DisassemblyOrderStatus.InProgress when order.Status == DisassemblyOrderStatus.NotStarted:
                    order.Start(); break;
                case DisassemblyOrderStatus.InProgress when order.Status == DisassemblyOrderStatus.Paused:
                    order.Resume(); break;
                case DisassemblyOrderStatus.Paused:
                    order.Pause(); break;
                case DisassemblyOrderStatus.Completed:
                    order.Complete(); break;
                case DisassemblyOrderStatus.Canceled:
                    order.Cancel(); break;
                default:
                    return ValidationResult<Unit>.Failure($"Invalid transition from '{order.Status}' to '{cmd.NewStatus}'.");
            }
            await repo.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
