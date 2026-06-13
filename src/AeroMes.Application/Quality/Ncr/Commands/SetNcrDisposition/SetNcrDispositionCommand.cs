using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Quality.Ncr.Commands.SetNcrDisposition;

public record SetNcrDispositionCommand(int NcrId, string DispositionCode, string SetBy)
    : ICommand<ValidationResult<Unit>>;
