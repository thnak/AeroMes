using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.Molds.Queries.GetMoldsDueForPm;

public record GetMoldsDueForPmQuery : IQuery<IReadOnlyList<MoldPmDueDto>>;

public record MoldPmDueDto(
    int MoldId,
    string MoldCode,
    string MoldName,
    string Status,
    long CurrentShots,
    long ShotsAtLastPm,
    int PmIntervalShots,
    long ShotsSinceLastPm,
    bool IsOverdue);
