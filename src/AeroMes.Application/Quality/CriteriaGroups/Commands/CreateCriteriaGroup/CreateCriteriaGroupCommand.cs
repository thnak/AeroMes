using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Quality.CriteriaGroups.Commands.CreateCriteriaGroup;

public record CreateCriteriaGroupCommand(string Code, string Name, string? CreatedBy)
    : ICommand<ValidationResult<int>>;
