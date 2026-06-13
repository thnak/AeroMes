using AeroMes.Application.Common;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Production.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.Commands.StartCutting;

public class StartCuttingHandler(
    ICutOrderRepository repo,
    IValidator<StartCuttingCommand> validator) : ICommandHandler<StartCuttingCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(StartCuttingCommand cmd, CancellationToken ct)
    {
        var vr = await validator.ValidateAsync(cmd, ct);
        if (!vr.IsValid) return ValidationResult<Unit>.Invalid(vr.ToErrorDictionary());

        var cutOrder = await repo.GetByIdAsync(cmd.CutOrderID, ct);
        if (cutOrder is null)
            return ValidationResult<Unit>.NotFound($"Cut order {cmd.CutOrderID} not found.");

        try
        {
            cutOrder.StartCutting();
            await repo.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
