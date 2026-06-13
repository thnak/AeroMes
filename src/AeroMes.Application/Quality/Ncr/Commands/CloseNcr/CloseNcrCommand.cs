using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Quality.Ncr.Commands.CloseNcr;

public record CloseNcrCommand(
    int NcrId,
    string ClosedBy,
    string? RootCause,
    string? CorrectiveAction,
    string? PreventiveAction)
    : ICommand<ValidationResult<Unit>>;
