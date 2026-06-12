using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Tools.Commands.RegisterTool;

public class RegisterToolHandler(
    IToolRepository repo,
    IUnitOfWork uow) : ICommandHandler<RegisterToolCommand, string>
{
    public async Task<string> HandleAsync(RegisterToolCommand cmd, CancellationToken ct)
    {
        var tool = Tool.Create(
            cmd.Code, cmd.Name, cmd.ToolType,
            cmd.Brand, cmd.Model, cmd.Specification,
            cmd.MaxUsageCount, cmd.PmIntervalCount,
            cmd.RequiresCalibration, cmd.CalibrationIntervalDays,
            cmd.StorageLocation, cmd.PurchaseDate, cmd.PurchaseCost,
            cmd.Notes, cmd.CreatedBy);
        await repo.AddAsync(tool, ct);
        await uow.SaveChangesAsync(ct);
        return tool.ToolCode;
    }
}
