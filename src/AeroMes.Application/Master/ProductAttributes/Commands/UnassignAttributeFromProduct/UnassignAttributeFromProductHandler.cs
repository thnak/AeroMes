using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.ProductAttributes.Commands.UnassignAttributeFromProduct;

public class UnassignAttributeFromProductHandler(
    IProductAttributeRepository repo,
    IUnitOfWork uow) : ICommandHandler<UnassignAttributeFromProductCommand>
{
    public async Task HandleAsync(UnassignAttributeFromProductCommand cmd, CancellationToken ct)
    {
        var assignment = await repo.GetAssignmentAsync(cmd.ProductCode, cmd.AttributeId, ct)
            ?? throw new EntityNotFoundException("ProductAttributeAssignment", $"{cmd.ProductCode}/{cmd.AttributeId}");

        assignment.SoftDelete(cmd.DeletedBy);
        await uow.SaveChangesAsync(ct);
    }
}
