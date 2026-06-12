using AeroMes.Domain.Master;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.Molds.Queries.GetMolds;

public record GetMoldsQuery(
    bool ActiveOnly = true,
    MoldStatus? Status = null,
    string? MachineCode = null,
    string? ProductCode = null,
    string? Search = null) : IQuery<IReadOnlyList<MoldDto>>;

public record MoldDto(
    int MoldId,
    string MoldCode,
    string MoldName,
    string MoldType,
    string? Material,
    int Cavities,
    long MaxShots,
    long CurrentShots,
    decimal ShotUtilizationPercent,
    bool PmDue,
    string Status,
    string? CurrentMachineCode,
    string? StorageLocation,
    string? DefaultProductCode,
    bool IsActive);
