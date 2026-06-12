using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Tools.Commands.RecordToolUsage;

public class RecordToolUsageHandler(
    IToolRepository repo,
    IUnitOfWork uow) : ICommandHandler<RecordToolUsageCommand, RecordToolUsageResult>
{
    public async Task<RecordToolUsageResult> HandleAsync(RecordToolUsageCommand cmd, CancellationToken ct)
    {
        var tool = await repo.GetByCodeAsync(cmd.ToolCode, ct)
            ?? throw new EntityNotFoundException(nameof(Tool), cmd.ToolCode);

        tool.AccumulateUsage(cmd.Count, cmd.UpdatedBy);
        await uow.SaveChangesAsync(ct);

        return new RecordToolUsageResult(
            tool.CurrentUsageCount, tool.MaxUsageCount,
            tool.IsReconditioningDue, tool.IsNearingEndOfLife);
    }
}
