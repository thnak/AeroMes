using AeroMes.Application.Interfaces;
using AeroMes.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Application.Downtime.Commands.StartDowntime;

public class StartDowntimeHandler(IApplicationDbContext db)
    : IRequestHandler<StartDowntimeCommand, long>
{
    public async Task<long> Handle(StartDowntimeCommand cmd, CancellationToken ct)
    {
        var exists = await db.WorkCenters.AnyAsync(x => x.WorkCenterID == cmd.WorkCenterId, ct);
        if (!exists)
            throw new KeyNotFoundException($"WorkCenter {cmd.WorkCenterId} not found.");

        var log = new DowntimeLog
        {
            WorkCenterID = cmd.WorkCenterId,
            MachineCode = cmd.MachineCode,
            ReasonCode = cmd.ReasonCode,
            ReasonName = cmd.ReasonName,
            StartTime = cmd.StartTime,
            OperatorID = cmd.OperatorId
        };
        db.DowntimeLogs.Add(log);
        await db.SaveChangesAsync(ct);

        return log.DowntimeLogID;
    }
}
