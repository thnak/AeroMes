using AeroMes.Application.Common;
using AeroMes.Domain.Quality;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Quality.Criteria.Commands.CreateCriteria;

public record CreateCriteriaCommand(
    string Code,
    string Name,
    int? GroupID,
    CriteriaType CriteriaType,
    string? InspectionMethod,
    string? MethodDescription,
    string? CreatedBy) : ICommand<ValidationResult<int>>;
