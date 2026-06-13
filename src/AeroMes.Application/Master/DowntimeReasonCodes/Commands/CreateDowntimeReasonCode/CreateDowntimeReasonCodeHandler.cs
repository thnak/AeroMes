using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.DowntimeReasonCodes.Commands.CreateDowntimeReasonCode;

public class CreateDowntimeReasonCodeHandler(
    IDowntimeReasonCodeRepository repo,
    IUnitOfWork uow,
    IValidator<CreateDowntimeReasonCodeCommand> validator) : ICommandHandler<CreateDowntimeReasonCodeCommand, ValidationResult<string>>
{
    public async Task<ValidationResult<string>> HandleAsync(CreateDowntimeReasonCodeCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<string>.Invalid(validation.ToErrorDictionary());

        try
        {
            var entity = DowntimeReasonCode.Create(
                cmd.Code, cmd.Name, cmd.Category,
                cmd.SlaMinutes, cmd.RequiresApproval, cmd.CreatedBy);
            await repo.AddAsync(entity, ct);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<string>.Ok(entity.ReasonCode);
        }
        catch (EntityNotFoundException ex)
        {
            return ValidationResult<string>.NotFound(ex.Message);
        }
        catch (DomainException ex)
        {
            return ValidationResult<string>.Failure(ex.Message);
        }
    }
}
