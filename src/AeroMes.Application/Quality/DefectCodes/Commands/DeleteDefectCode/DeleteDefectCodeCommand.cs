using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Quality.DefectCodes.Commands.DeleteDefectCode;

public record DeleteDefectCodeCommand(int Id, string? DeletedBy = null) : ICommand;
