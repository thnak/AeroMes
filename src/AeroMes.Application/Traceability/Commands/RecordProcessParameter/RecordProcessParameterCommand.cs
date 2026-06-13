using AeroMes.Application.Common;
using AeroMes.Domain.Traceability;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Traceability.Commands.RecordProcessParameter;

public sealed record RecordProcessParameterCommand(
    Guid ProcessRecordID,
    string ParameterName,
    string ActualValue,
    string? NominalValue,
    string? UoM,
    string? LSL,
    string? USL,
    ParameterDataSource DataSource)
    : ICommand<ValidationResult<Unit>>;
