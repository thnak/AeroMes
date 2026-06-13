using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Quality.Ncr.Commands.EscalateNcr;

public record EscalateNcrCommand(int NcrId) : ICommand<ValidationResult<Unit>>;
