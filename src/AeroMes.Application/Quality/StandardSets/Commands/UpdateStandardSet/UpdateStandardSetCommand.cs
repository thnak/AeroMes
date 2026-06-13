using AeroMes.Application.Common;
using AeroMes.Application.Quality.StandardSets.Commands.CreateStandardSet;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Quality.StandardSets.Commands.UpdateStandardSet;

public record UpdateStandardSetCommand(
    int StandardSetID, string Name, int SamplingMethodID,
    DateOnly EffectiveDate, string? Notes,
    IReadOnlyList<ProductCriteriaInput> ProductCriteria,
    IReadOnlyList<StageCriteriaInput> StageCriteria,
    string? UpdatedBy)
    : ICommand<ValidationResult<Unit>>;
