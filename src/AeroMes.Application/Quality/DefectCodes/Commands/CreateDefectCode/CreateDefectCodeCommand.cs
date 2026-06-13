using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Quality.DefectCodes.Commands.CreateDefectCode;

public record CreateDefectCodeCommand(
    string Code,
    string DefectName,
    string? DefectCategory,
    string? CreatedBy) : ICommand<ValidationResult<int>>;
