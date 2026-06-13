using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Maintenance.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Maintenance.Commands.UpdateMaintenanceOrderStatus;

public class UpdateMaintenanceOrderStatusHandler(
    IMaintenanceOrderRepository repository,
    IUnitOfWork uow)
    : ICommandHandler<UpdateMaintenanceOrderStatusCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(UpdateMaintenanceOrderStatusCommand command, CancellationToken ct)
    {
        var order = await repository.GetByIdAsync(command.MaintOrderID, ct);
        if (order is null) return ValidationResult<Unit>.NotFound($"Lệnh bảo trì #{command.MaintOrderID} không tìm thấy.");

        try
        {
            switch (command.Action.ToLowerInvariant())
            {
                case "start": order.Start(command.UpdatedBy); break;
                case "hold": order.HoldForParts(command.UpdatedBy); break;
                case "complete": order.Complete(command.UpdatedBy); break;
                case "cancel": order.Cancel(command.UpdatedBy); break;
                default:
                    return ValidationResult<Unit>.Failure($"Hành động '{command.Action}' không hợp lệ. Dùng: start, hold, complete, cancel.");
            }
            await uow.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (DomainException ex) { return ValidationResult<Unit>.Failure(ex.Message); }
    }
}
