using AeroMes.Application.Common;
using AeroMes.Domain.Quality;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Quality.StandardSets.Commands.SetStandardSetStatus;

public record SetStandardSetStatusCommand(int StandardSetID, StandardSetStatus Status, string? UpdatedBy)
    : ICommand<ValidationResult<Unit>>;
