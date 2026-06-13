using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Iot.Signals.Commands.UpdateSignal;

public record UpdateSignalCommand(
    int Id,
    string DisplayName,
    string SourceAddress,
    double Scale,
    double Offset,
    double? QualityMin,
    double? QualityMax,
    string UpdatedBy) : ICommand<ValidationResult<int>>;
