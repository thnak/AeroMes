using AeroMes.Application.Common;
using AeroMes.Domain.Quality;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Quality.DefectCodes.Commands.CreateDefectCode;

public record CreateDefectCodeCommand(
    string Code,
    string DefectName,
    string? DefectCategory,
    string? CreatedBy,
    bool IsRepairable = false,
    DefectSeverityLevel SeverityLevel = DefectSeverityLevel.Minor,
    string? Description = null) : ICommand<ValidationResult<int>>;
