using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Molds.Commands.RecordMoldShots;

public record RecordMoldShotsCommand(
    string MoldCode,
    long Shots,
    string? UpdatedBy) : ICommand<RecordMoldShotsResult>;

public record RecordMoldShotsResult(
    long CurrentShots,
    long MaxShots,
    bool PmDue,
    bool NearingEndOfLife);
