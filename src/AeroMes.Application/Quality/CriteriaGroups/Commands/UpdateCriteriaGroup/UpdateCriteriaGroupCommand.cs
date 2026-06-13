using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Quality.CriteriaGroups.Commands.UpdateCriteriaGroup;

public record UpdateCriteriaGroupCommand(int GroupID, string Name, string? UpdatedBy)
    : ICommand<ValidationResult<Unit>>;
