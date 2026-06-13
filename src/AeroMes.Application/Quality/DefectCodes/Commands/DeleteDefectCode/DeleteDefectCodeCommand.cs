using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Quality.DefectCodes.Commands.DeleteDefectCode;

public record DeleteDefectCodeCommand(int Id, string? DeletedBy = null) : ICommand<ValidationResult<Unit>>;
