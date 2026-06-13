using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Quality.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Quality.Ncr.Commands.CloseNcr;

public class CloseNcrHandler(
    INcrRepository ncrRepo,
    IUnitOfWork uow,
    IValidator<CloseNcrCommand> validator)
    : ICommandHandler<CloseNcrCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(CloseNcrCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<Unit>.Invalid(validation.ToErrorDictionary());

        try
        {
            var ncr = await ncrRepo.GetByIdAsync(cmd.NcrId, ct);
            if (ncr is null)
                return ValidationResult<Unit>.NotFound($"NCR '{cmd.NcrId}' was not found.");

            ncr.Close(cmd.ClosedBy, cmd.RootCause, cmd.CorrectiveAction, cmd.PreventiveAction);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
