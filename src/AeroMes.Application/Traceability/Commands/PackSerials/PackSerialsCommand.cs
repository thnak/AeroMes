using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Traceability.Commands.PackSerials;

public record PackSerialsCommand(
    IReadOnlyList<string> SerialNumbers,
    string CaseSSCC,
    string? PalletSSCC,
    string OperatorCode
) : ICommand<ValidationResult<int>>;
