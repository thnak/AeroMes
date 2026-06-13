using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Quality.StandardSets.Commands.DeleteStandardSet;

public record DeleteStandardSetCommand(int StandardSetID) : ICommand<ValidationResult<Unit>>;
