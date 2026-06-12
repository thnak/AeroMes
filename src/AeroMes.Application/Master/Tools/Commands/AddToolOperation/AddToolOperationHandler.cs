using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Tools.Commands.AddToolOperation;

public class AddToolOperationHandler(
    IToolRepository repo,
    IUnitOfWork uow) : ICommandHandler<AddToolOperationCommand, int>
{
    public async Task<int> HandleAsync(AddToolOperationCommand cmd, CancellationToken ct)
    {
        var tool = await repo.GetByCodeAsync(cmd.ToolCode, ct)
            ?? throw new EntityNotFoundException(nameof(Tool), cmd.ToolCode);

        var mapping = tool.AddOperationMapping(
            cmd.OperationCode, cmd.ProductCode, cmd.IsRequired, cmd.UsageCountPerOp, cmd.UpdatedBy);
        await uow.SaveChangesAsync(ct);
        return mapping.MappingId;
    }
}
