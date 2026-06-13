using AeroMes.Application.Common;
using AeroMes.Domain.Quality;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Quality.DefectCodes.Commands.UpdateDefectCode;

public record UpdateDefectCodeCommand(
    int Id,
    string DefectName,
    string? DefectCategory,
    bool IsActive,
    bool IsRepairable,
    string? UpdatedBy,
    DefectSeverityLevel SeverityLevel = DefectSeverityLevel.Minor,
    string? Description = null) : ICommand<ValidationResult<Unit>>;
