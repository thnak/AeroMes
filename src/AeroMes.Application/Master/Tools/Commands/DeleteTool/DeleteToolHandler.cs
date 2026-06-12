using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Tools.Commands.DeleteTool;

public class DeleteToolHandler(
    IToolRepository repo,
    IUnitOfWork uow) : ICommandHandler<DeleteToolCommand>
{
    public async Task HandleAsync(DeleteToolCommand cmd, CancellationToken ct)
    {
        var tool = await repo.GetByCodeAsync(cmd.ToolCode, ct)
            ?? throw new EntityNotFoundException(nameof(Tool), cmd.ToolCode);

        if (tool.Status == ToolStatus.InUse)
            throw new DomainException(
                $"Phải trả dụng cụ '{tool.ToolCode}' về kho trước khi xóa.");

        tool.SoftDelete(cmd.DeletedBy);
        await uow.SaveChangesAsync(ct);
    }
}
