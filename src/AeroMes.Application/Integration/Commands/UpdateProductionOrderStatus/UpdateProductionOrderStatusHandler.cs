using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Integration.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Integration.Commands.UpdateProductionOrderStatus;

public sealed class UpdateProductionOrderStatusHandler(
    IProductionOrderRepository repo,
    IUnitOfWork uow)
    : ICommandHandler<UpdateProductionOrderStatusCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        UpdateProductionOrderStatusCommand cmd, CancellationToken ct = default)
    {
        var po = await repo.GetByIdAsync(cmd.Id, ct);
        if (po is null) return ValidationResult<Unit>.NotFound($"ProductionOrder '{cmd.Id}' was not found.");

        try
        {
            switch (cmd.Action.Trim().ToLowerInvariant())
            {
                case "start":   po.SetRunning(); break;
                case "pause":   po.Pause(); break;
                case "resume":  po.Resume(); break;
                case "complete": po.Complete(); break;
                case "cancel":  po.Cancel(); break;
                default:
                    return ValidationResult<Unit>.Failure(
                        $"Action không hợp lệ: '{cmd.Action}'. Hợp lệ: Start, Pause, Resume, Complete, Cancel.");
            }
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }

        await uow.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
