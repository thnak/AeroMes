using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Maintenance;
using AeroMes.Domain.Maintenance.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Maintenance.Commands.AddMaintCostLine;

public class AddMaintCostLineHandler(
    IMaintenanceOrderRepository repository,
    IUnitOfWork uow)
    : ICommandHandler<AddMaintCostLineCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(AddMaintCostLineCommand command, CancellationToken ct)
    {
        var order = await repository.GetByIdAsync(command.MaintOrderID, ct);
        if (order is null) return ValidationResult<Unit>.NotFound($"Lệnh bảo trì #{command.MaintOrderID} không tìm thấy.");

        try
        {
            var line = MaintCostLine.Create(
                command.MaintOrderID, command.CostCategory,
                command.ProductCode, command.LotNumber, command.QtyUsed, command.UnitCost,
                command.OperatorID, command.LaborHours, command.LaborRateSnapshot,
                command.SupplierID, command.InvoiceRef, command.InvoiceAmount,
                command.PostedBy);
            order.AddCostLine(line, command.PostedBy);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (DomainException ex) { return ValidationResult<Unit>.Failure(ex.Message); }
    }
}
