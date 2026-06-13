using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Quality.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Quality.Ncr.Commands.UpdateNcr;

public class UpdateNcrHandler(
    INcrRepository ncrRepo,
    IUnitOfWork uow,
    IValidator<UpdateNcrCommand> validator)
    : ICommandHandler<UpdateNcrCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(UpdateNcrCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<Unit>.Invalid(validation.ToErrorDictionary());

        var ncr = await ncrRepo.GetByIdAsync(cmd.NcrId, ct);
        if (ncr is null)
            return ValidationResult<Unit>.NotFound($"NCR '{cmd.NcrId}' was not found.");

        ncr.Update(cmd.AssignedTo, cmd.DueDate);
        await uow.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
