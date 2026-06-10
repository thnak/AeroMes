using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Production;
using AeroMes.Domain.Production.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Downtime.Commands.EndDowntime;

public class EndDowntimeHandler(
    IDowntimeLogRepository downtimeRepo,
    IUnitOfWork uow)
    : ICommandHandler<EndDowntimeCommand, EndDowntimeResult>
{
    public async Task<EndDowntimeResult> HandleAsync(EndDowntimeCommand cmd, CancellationToken ct)
    {
        var log = await downtimeRepo.GetByIdAsync(cmd.DowntimeLogId, ct)
            ?? throw new EntityNotFoundException(nameof(DowntimeLog), cmd.DowntimeLogId);

        log.End(cmd.EndTime, cmd.Notes);
        await uow.SaveChangesAsync(ct);

        return new EndDowntimeResult(log.DowntimeLogID, log.DurationMinutes!.Value, "RESOLVED");
    }
}
