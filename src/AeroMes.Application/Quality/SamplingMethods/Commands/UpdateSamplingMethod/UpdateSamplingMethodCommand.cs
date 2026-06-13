using AeroMes.Application.Common;
using AeroMes.Application.Quality.SamplingMethods.Commands.CreateSamplingMethod;
using AeroMes.Domain.Quality;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Quality.SamplingMethods.Commands.UpdateSamplingMethod;

public record UpdateSamplingMethodCommand(
    int SamplingMethodID,
    string Name,
    SamplingType SamplingType,
    decimal? SampleQuantity,
    int MaxDefects,
    SamplingMethodStatus Status,
    IReadOnlyList<VolumeRangeInput>? VolumeRanges,
    string? Notes) : ICommand<ValidationResult<Unit>>;
