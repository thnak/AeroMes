using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Production;
using AeroMes.Domain.Production.Repositories;
using MediatR;

namespace AeroMes.Application.Downtime.Commands.EndDowntime;

public class EndDowntimeHandler(
    IDowntimeLogRepository downtimeRepo,
    IUnitOfWork uow)
    : IRequestHandler<EndDowntimeCommand, EndDowntimeResult>
{
    public async Task<EndDowntimeResult> Handle(EndDowntimeCommand cmd, CancellationToken ct)
    {
        var log = await downtimeRepo.GetByIdAsync(cmd.DowntimeLogId, ct)
            ?? throw new EntityNotFoundException(nameof(DowntimeLog), cmd.DowntimeLogId);

        log.End(cmd.EndTime, cmd.Notes);
        await uow.SaveChangesAsync(ct);

        return new EndDowntimeResult(log.DowntimeLogID, log.DurationMinutes!.Value, "RESOLVED");
    }
}
