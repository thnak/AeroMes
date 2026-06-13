using AeroMes.Application.Common;
using AeroMes.Domain.Quality;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Quality.Criteria.Commands.UpdateCriteria;

public record UpdateCriteriaCommand(
    int CriteriaID,
    string Name,
    int? GroupID,
    CriteriaType CriteriaType,
    string? InspectionMethod,
    string? MethodDescription,
    string? UpdatedBy) : ICommand<ValidationResult<Unit>>;
