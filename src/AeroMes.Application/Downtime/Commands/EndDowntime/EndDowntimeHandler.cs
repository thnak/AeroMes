using AeroMes.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Application.Downtime.Commands.EndDowntime;

public class EndDowntimeHandler(IApplicationDbContext db)
    : IRequestHandler<EndDowntimeCommand, EndDowntimeResult>
{
    public async Task<EndDowntimeResult> Handle(EndDowntimeCommand cmd, CancellationToken ct)
    {
        var log = await db.DowntimeLogs
            .FirstOrDefaultAsync(x => x.DowntimeLogID == cmd.DowntimeLogId, ct)
            ?? throw new KeyNotFoundException($"DowntimeLog {cmd.DowntimeLogId} not found.");

        if (log.EndTime.HasValue)
            throw new InvalidOperationException($"DowntimeLog {cmd.DowntimeLogId} is already closed.");

        log.EndTime = cmd.EndTime;
        await db.SaveChangesAsync(ct);

        return new EndDowntimeResult(log.DowntimeLogID, log.DurationMinutes ?? 0);
    }
}
