using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Quality.Ncr.Commands.CancelNcr;

public record CancelNcrCommand(int NcrId, string CancelledBy) : ICommand<ValidationResult<Unit>>;
