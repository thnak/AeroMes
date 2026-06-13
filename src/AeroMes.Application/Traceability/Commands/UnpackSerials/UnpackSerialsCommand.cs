using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Traceability.Commands.UnpackSerials;

public record UnpackSerialsCommand(
    string SSCC,
    string OperatorCode
) : ICommand<ValidationResult<int>>;
