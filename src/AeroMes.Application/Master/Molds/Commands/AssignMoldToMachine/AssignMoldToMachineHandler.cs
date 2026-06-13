using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Application.Master.Molds.Commands.AssignMoldToMachine;

public class AssignMoldToMachineHandler(
    IMoldRepository repo,
    IUnitOfWork uow,
    IValidator<AssignMoldToMachineCommand> validator) : ICommandHandler<AssignMoldToMachineCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(AssignMoldToMachineCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<Unit>.Invalid(validation.ToErrorDictionary());

        try
        {
            var mold = await repo.GetByCodeAsync(cmd.MoldCode, ct);
            if (mold is null) return ValidationResult<Unit>.NotFound($"Entity '{cmd.MoldCode}' was not found.");

            mold.AssignToMachine(cmd.MachineCode, cmd.UpdatedBy);
            await uow.SaveChangesAsync(ct);

            return ValidationResult<Unit>.Ok(Unit.Value);
        }        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
