using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Import.Commands.ValidateImport;

public record ValidateImportResult(
    int ImportJobId,
    int TotalRows,
    int ValidRows,
    int InvalidRows,
    IReadOnlyList<ImportRowError> Errors);

public record ValidateImportCommand(
    Stream FileStream,
    string FileName,
    string Category,
    int StartRow,
    string? CreatedBy) : ICommand<ValidationResult<ValidateImportResult>>;
