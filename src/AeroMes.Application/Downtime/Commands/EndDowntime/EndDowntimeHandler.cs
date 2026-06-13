using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Production;
using AeroMes.Domain.Production.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Application.Downtime.Commands.EndDowntime;

public class EndDowntimeHandler(
    IDowntimeLogRepository downtimeRepo,
    IUnitOfWork uow,
    IValidator<EndDowntimeCommand> validator)
    : ICommandHandler<EndDowntimeCommand, ValidationResult<EndDowntimeResult>>
{
    public async Task<ValidationResult<EndDowntimeResult>> HandleAsync(EndDowntimeCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<EndDowntimeResult>.Invalid(validation.ToErrorDictionary());

        try
        {
            var log = await downtimeRepo.GetByIdAsync(cmd.DowntimeLogId, ct);
            if (log is null) return ValidationResult<EndDowntimeResult>.NotFound($"Entity '{cmd.DowntimeLogId}' was not found.");

            log.End(cmd.EndTime, cmd.Notes);
            await uow.SaveChangesAsync(ct);

            return ValidationResult<EndDowntimeResult>.Ok(new EndDowntimeResult(log.DowntimeLogID, log.DurationMinutes!.Value, "RESOLVED"));
        }        catch (DomainException ex)
        {
            return ValidationResult<EndDowntimeResult>.Failure(ex.Message);
        }
    }
}
