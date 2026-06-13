using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Production;
using AeroMes.Domain.Production.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Downtime.Commands.StartDowntime;

public class StartDowntimeHandler(
    IDowntimeLogRepository downtimeRepo,
    IUnitOfWork uow,
    IValidator<StartDowntimeCommand> validator)
    : ICommandHandler<StartDowntimeCommand, ValidationResult<long>>
{
    public async Task<ValidationResult<long>> HandleAsync(StartDowntimeCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<long>.Invalid(validation.ToErrorDictionary());

        try
        {
            var log = DowntimeLog.Create(
                cmd.MachineCode, cmd.ReasonCode, cmd.ReasonName,
                cmd.StartTime, cmd.OperatorId, cmd.Notes);

            await downtimeRepo.AddAsync(log, ct);
            await uow.SaveChangesAsync(ct);

            return ValidationResult<long>.Ok(log.DowntimeLogID);
        }
        catch (EntityNotFoundException ex)
        {
            return ValidationResult<long>.NotFound(ex.Message);
        }
        catch (DomainException ex)
        {
            return ValidationResult<long>.Failure(ex.Message);
        }
    }
}
