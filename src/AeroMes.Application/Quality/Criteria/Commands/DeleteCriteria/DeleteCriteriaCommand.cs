using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Quality.Criteria.Commands.DeleteCriteria;

public record DeleteCriteriaCommand(int CriteriaID) : ICommand<ValidationResult<Unit>>;
