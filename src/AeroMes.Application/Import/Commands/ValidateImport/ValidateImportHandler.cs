using AeroMes.Application.Common;
using AeroMes.Domain.Import;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Import.Commands.ValidateImport;

public sealed class ValidateImportHandler(IImportService importService, IImportRepository repo)
    : ICommandHandler<ValidateImportCommand, ValidationResult<ValidateImportResult>>
{
    public async Task<ValidationResult<ValidateImportResult>> HandleAsync(
        ValidateImportCommand cmd, CancellationToken ct = default)
    {
        var supported = importService.GetSupportedCategories()
            .Select(c => c.Category)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (!supported.Contains(cmd.Category))
            return ValidationResult<ValidateImportResult>.Failure(
                $"Danh mục nhập khẩu '{cmd.Category}' không được hỗ trợ. Hỗ trợ: {string.Join(", ", supported)}");

        var validation = await importService.ValidateAsync(
            cmd.FileStream, cmd.FileName, cmd.Category, cmd.StartRow, ct);

        var job = ImportJob.Create(
            cmd.Category, cmd.FileName,
            validation.TotalRows, validation.ValidRows, validation.InvalidRows,
            validation.ValidRowsJson, validation.ErrorRowsJson, cmd.CreatedBy);

        var jobId = await repo.AddAsync(job, ct);

        return ValidationResult<ValidateImportResult>.Ok(new ValidateImportResult(
            jobId, validation.TotalRows, validation.ValidRows,
            validation.InvalidRows, validation.Errors));
    }
}
