using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Quality.SamplingMethods.Commands.DeleteSamplingMethod;

public record DeleteSamplingMethodCommand(int SamplingMethodID) : ICommand<ValidationResult<Unit>>;
