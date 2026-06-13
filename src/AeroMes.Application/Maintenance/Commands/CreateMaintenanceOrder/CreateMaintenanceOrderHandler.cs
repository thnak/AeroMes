using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Maintenance;
using AeroMes.Domain.Maintenance.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Maintenance.Commands.CreateMaintenanceOrder;

public class CreateMaintenanceOrderHandler(
    IMaintenanceOrderRepository repository,
    IUnitOfWork uow,
    IValidator<CreateMaintenanceOrderCommand> validator)
    : ICommandHandler<CreateMaintenanceOrderCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(CreateMaintenanceOrderCommand command, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(command, ct);
        if (!v.IsValid) return ValidationResult<int>.Invalid(v.ToErrorDictionary());

        if (await repository.CodeExistsAsync(command.MaintOrderCode, ct))
            return ValidationResult<int>.Failure($"Mã lệnh '{command.MaintOrderCode}' đã tồn tại.");

        try
        {
            var order = MaintenanceOrder.Create(
                command.MaintOrderCode, command.MachineCode, command.OrderType,
                command.TriggerRef, command.Priority, command.PlannedStartAt,
                command.PlannedEndAt, command.AssignedTo, command.EstimatedCost,
                command.Notes, command.CreatedBy);
            await repository.AddAsync(order, ct);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<int>.Ok(order.MaintOrderID);
        }
        catch (DomainException ex) { return ValidationResult<int>.Failure(ex.Message); }
    }
}
