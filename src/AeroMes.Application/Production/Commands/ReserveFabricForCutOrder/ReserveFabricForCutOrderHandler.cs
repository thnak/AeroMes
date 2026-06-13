using AeroMes.Application.Common;
using AeroMes.Application.Master.FabricRolls.Commands.ReserveFabricRolls;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Production.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.Commands.ReserveFabricForCutOrder;

public class ReserveFabricForCutOrderHandler(
    ICutOrderRepository repo,
    ICommandMediator commandMediator,
    IValidator<ReserveFabricForCutOrderCommand> validator)
    : ICommandHandler<ReserveFabricForCutOrderCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(ReserveFabricForCutOrderCommand cmd, CancellationToken ct)
    {
        var vr = await validator.ValidateAsync(cmd, ct);
        if (!vr.IsValid) return ValidationResult<Unit>.Invalid(vr.ToErrorDictionary());

        var cutOrder = await repo.GetByIdAsync(cmd.CutOrderID, ct);
        if (cutOrder is null)
            return ValidationResult<Unit>.NotFound($"Cut order {cmd.CutOrderID} not found.");

        var reserveResult = await commandMediator.SendAsync(
            new ReserveFabricRollsCommand(cmd.CutOrderID, cmd.RollIDs), null, ct);
        if (!reserveResult.IsSuccess) return reserveResult;

        try
        {
            cutOrder.ReserveFabric(cmd.RollIDs);
            await repo.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
