using AeroMes.Application.Common;
using AeroMes.Domain.Quality;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Quality.Criteria.Commands.SetCriteriaStatus;

public record SetCriteriaStatusCommand(int CriteriaID, CriteriaStatus Status, string? UpdatedBy)
    : ICommand<ValidationResult<Unit>>;
