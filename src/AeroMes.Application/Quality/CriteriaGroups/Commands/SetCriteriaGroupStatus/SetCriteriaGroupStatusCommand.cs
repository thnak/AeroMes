using AeroMes.Application.Common;
using AeroMes.Domain.Quality;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Quality.CriteriaGroups.Commands.SetCriteriaGroupStatus;

public record SetCriteriaGroupStatusCommand(int GroupID, CriteriaGroupStatus Status, string? UpdatedBy)
    : ICommand<ValidationResult<Unit>>;
