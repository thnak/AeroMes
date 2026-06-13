using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Quality.StandardSets.Commands.CreateStandardSet;

public record ProductCriteriaInput(int CriteriaId, string? Parameters);
public record StageCriteriaInput(int StageId, int CriteriaId, int? SamplingMethodId, string? Parameters);

public record CreateStandardSetCommand(
    string Code, string Name, string ProductCode,
    int SamplingMethodID, DateOnly EffectiveDate, string? Notes,
    IReadOnlyList<ProductCriteriaInput> ProductCriteria,
    IReadOnlyList<StageCriteriaInput> StageCriteria,
    string? CreatedBy)
    : ICommand<ValidationResult<int>>;
