using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Quality.Ncr.Commands.UpdateNcr;

public record UpdateNcrCommand(int NcrId, string? AssignedTo, DateTimeOffset? DueDate)
    : ICommand<ValidationResult<Unit>>;
