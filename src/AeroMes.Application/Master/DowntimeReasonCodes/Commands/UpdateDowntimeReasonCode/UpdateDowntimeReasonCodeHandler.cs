using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Application.Master.DowntimeReasonCodes.Commands.UpdateDowntimeReasonCode;

public class UpdateDowntimeReasonCodeHandler(
    IDowntimeReasonCodeRepository repo,
    IUnitOfWork uow,
    IValidator<UpdateDowntimeReasonCodeCommand> validator) : ICommandHandler<UpdateDowntimeReasonCodeCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(UpdateDowntimeReasonCodeCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<Unit>.Invalid(validation.ToErrorDictionary());

        try
        {
            var entity = await repo.GetByCodeAsync(cmd.Code, ct);
            if (entity is null)
                return ValidationResult<Unit>.NotFound($"DowntimeReasonCode '{cmd.Code}' not found.");

            entity.UpdateDetails(cmd.Name, cmd.Category, cmd.SlaMinutes, cmd.RequiresApproval, cmd.IsActive, cmd.UpdatedBy);
            await uow.SaveChangesAsync(ct);

            return ValidationResult<Unit>.Ok(Unit.Value);
        }        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
