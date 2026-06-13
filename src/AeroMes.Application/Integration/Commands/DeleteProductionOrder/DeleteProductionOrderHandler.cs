using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Integration;
using AeroMes.Domain.Integration.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Integration.Commands.DeleteProductionOrder;

public sealed class DeleteProductionOrderHandler(
    IProductionOrderRepository repo,
    IUnitOfWork uow)
    : ICommandHandler<DeleteProductionOrderCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        DeleteProductionOrderCommand cmd, CancellationToken ct = default)
    {
        var po = await repo.GetByIdAsync(cmd.Id, ct);
        if (po is null) return ValidationResult<Unit>.NotFound($"ProductionOrder '{cmd.Id}' was not found.");

        if (po.Status == ProductionOrderStatus.Completed)
            return ValidationResult<Unit>.Failure("Không thể xóa lệnh sản xuất đã hoàn thành.");

        if (await repo.HasDownstreamDocumentsAsync(cmd.Id, ct))
            return ValidationResult<Unit>.Failure(
                "Lệnh sản xuất đã phát sinh chứng từ (Work Orders, MR...) — không thể xóa.");

        repo.Remove(po);
        await uow.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
