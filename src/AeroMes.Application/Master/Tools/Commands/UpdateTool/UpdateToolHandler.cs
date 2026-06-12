using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Tools.Commands.UpdateTool;

public class UpdateToolHandler(
    IToolRepository repo,
    IUnitOfWork uow) : ICommandHandler<UpdateToolCommand>
{
    public async Task HandleAsync(UpdateToolCommand cmd, CancellationToken ct)
    {
        var tool = await repo.GetByCodeAsync(cmd.Code, ct)
            ?? throw new EntityNotFoundException(nameof(Tool), cmd.Code);

        tool.UpdateDetails(
            cmd.Name, cmd.ToolType,
            cmd.Brand, cmd.Model, cmd.Specification,
            cmd.MaxUsageCount, cmd.PmIntervalCount,
            cmd.RequiresCalibration, cmd.CalibrationIntervalDays,
            cmd.StorageLocation, cmd.PurchaseDate, cmd.PurchaseCost,
            cmd.Notes, cmd.IsActive, cmd.UpdatedBy);
        await uow.SaveChangesAsync(ct);
    }
}
