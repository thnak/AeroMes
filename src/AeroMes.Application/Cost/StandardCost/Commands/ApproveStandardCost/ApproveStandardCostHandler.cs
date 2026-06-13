using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Cost.Repositories;
using AeroMes.Domain.Exceptions;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Cost.StandardCost.Commands.ApproveStandardCost;

public sealed class ApproveStandardCostHandler(
    IStandardCostRepository repo,
    IUnitOfWork uow)
    : ICommandHandler<ApproveStandardCostCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        ApproveStandardCostCommand cmd, CancellationToken ct = default)
    {
        var header = await repo.GetByIdAsync(cmd.StdCostId, ct);
        if (header is null) return ValidationResult<Unit>.NotFound($"StandardCost '{cmd.StdCostId}' was not found.");

        try { header.Approve(cmd.ApprovedBy); }
        catch (DomainException ex) { return ValidationResult<Unit>.Failure(ex.Message); }

        await uow.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
