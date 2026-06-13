using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Quality.DefectCodes.Commands.UpdateDefectCode;

public record UpdateDefectCodeCommand(
    int Id,
    string DefectName,
    string? DefectCategory,
    bool IsActive,
    string? UpdatedBy) : ICommand<ValidationResult<Unit>>;
