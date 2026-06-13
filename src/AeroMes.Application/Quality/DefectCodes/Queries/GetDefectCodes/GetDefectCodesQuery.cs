using AeroMes.Domain.Quality;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Quality.DefectCodes.Queries.GetDefectCodes;

public record GetDefectCodesQuery(bool ActiveOnly = true) : IQuery<IReadOnlyList<DefectCodeDto>>;

public record DefectCodeDto(
    int DefectCodeId,
    string Code,
    string DefectName,
    string? DefectCategory,
    bool IsActive,
    bool IsRepairable,
    string SeverityLevel,
    string? Description);
