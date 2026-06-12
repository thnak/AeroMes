using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Tools.Commands.CheckoutTool;

public class CheckoutToolHandler(
    IToolRepository repo,
    IUnitOfWork uow) : ICommandHandler<CheckoutToolCommand, long>
{
    public async Task<long> HandleAsync(CheckoutToolCommand cmd, CancellationToken ct)
    {
        var tool = await repo.GetByCodeAsync(cmd.ToolCode, ct)
            ?? throw new EntityNotFoundException(nameof(Tool), cmd.ToolCode);

        var checkout = tool.Checkout(
            cmd.WorkCenterId, cmd.CheckedOutBy, cmd.ExpectedReturnAt, cmd.UpdatedBy);
        await uow.SaveChangesAsync(ct);
        return checkout.CheckoutId;
    }
}
