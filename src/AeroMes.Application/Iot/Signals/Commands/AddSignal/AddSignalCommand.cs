using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Iot.Signals.Commands.AddSignal;

public record AddSignalCommand(
    int AdapterId,
    string TagKey,
    string DisplayName,
    string SourceAddress,
    double Scale,
    double Offset,
    double? QualityMin,
    double? QualityMax,
    string? CreatedBy) : ICommand<ValidationResult<int>>;
