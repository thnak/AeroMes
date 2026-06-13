using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Cost.Repositories;
using AeroMes.Domain.Exceptions;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Cost.StandardCost.Commands.ActivateStandardCost;

public sealed class ActivateStandardCostHandler(
    IStandardCostRepository repo,
    IUnitOfWork uow)
    : ICommandHandler<ActivateStandardCostCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        ActivateStandardCostCommand cmd, CancellationToken ct = default)
    {
        var header = await repo.GetByIdAsync(cmd.StdCostId, ct);
        if (header is null) return ValidationResult<Unit>.NotFound($"StandardCost '{cmd.StdCostId}' was not found.");

        // Supersede any existing Active version for this product
        var existing = await repo.GetActiveByProductAsync(header.ProductCode, ct);
        if (existing is not null && existing.StdCostId != cmd.StdCostId)
            existing.Supersede(cmd.EffectiveFrom.AddDays(-1));

        try { header.Activate(cmd.EffectiveFrom); }
        catch (DomainException ex) { return ValidationResult<Unit>.Failure(ex.Message); }

        await uow.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
