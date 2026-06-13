using AeroMes.Application.Common;
using AeroMes.Domain.Quality;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Quality.SamplingMethods.Commands.CreateSamplingMethod;

public record VolumeRangeInput(int MinQty, int MaxQty, decimal SampleSizeOrRatio, int MaxDefects);

public record CreateSamplingMethodCommand(
    string Code,
    string Name,
    SamplingType SamplingType,
    decimal? SampleQuantity,
    int MaxDefects,
    IReadOnlyList<VolumeRangeInput>? VolumeRanges,
    string? Notes) : ICommand<ValidationResult<int>>;
