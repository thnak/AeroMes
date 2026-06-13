using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Master.ProductAttributes.Commands.UnassignAttributeFromProduct;

public class UnassignAttributeFromProductHandler(
    IProductAttributeRepository repo,
    IUnitOfWork uow) : ICommandHandler<UnassignAttributeFromProductCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(UnassignAttributeFromProductCommand cmd, CancellationToken ct)
    {
        var assignment = await repo.GetAssignmentAsync(cmd.ProductCode, cmd.AttributeId, ct);
        if (assignment is null) return ValidationResult<Unit>.NotFound($"ProductAttributeAssignment '{$"{cmd.ProductCode}/{cmd.AttributeId}"}' was not found.");

        assignment.SoftDelete(cmd.DeletedBy);
        await uow.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
