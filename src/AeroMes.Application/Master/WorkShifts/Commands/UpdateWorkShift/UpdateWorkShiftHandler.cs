using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Application.Master.WorkShifts.Commands.UpdateWorkShift;

public class UpdateWorkShiftHandler(
    IWorkShiftRepository repo,
    IUnitOfWork uow,
    IValidator<UpdateWorkShiftCommand> validator) : ICommandHandler<UpdateWorkShiftCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(UpdateWorkShiftCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<Unit>.Invalid(validation.ToErrorDictionary());

        try
        {
            var entity = await repo.GetByIdWithBreaksAsync(cmd.WorkShiftId, ct);
            if (entity is null)
                return ValidationResult<Unit>.NotFound($"Work shift {cmd.WorkShiftId} not found.");
            entity.UpdateDetails(
                cmd.Name, cmd.StartTime, cmd.EndTime,
                cmd.Breaks.Select(b => (b.BreakStart, b.BreakEnd)),
                cmd.IsActive, cmd.UpdatedBy);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
