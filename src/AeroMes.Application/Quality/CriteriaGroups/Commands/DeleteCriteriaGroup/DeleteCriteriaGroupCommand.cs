using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Quality.CriteriaGroups.Commands.DeleteCriteriaGroup;

public record DeleteCriteriaGroupCommand(int GroupID) : ICommand<ValidationResult<Unit>>;
