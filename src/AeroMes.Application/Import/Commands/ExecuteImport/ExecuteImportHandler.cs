using AeroMes.Application.Common;
using AeroMes.Domain.Import;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Import.Commands.ExecuteImport;

public sealed class ExecuteImportHandler(IImportService importService, IImportRepository repo)
    : ICommandHandler<ExecuteImportCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(
        ExecuteImportCommand cmd, CancellationToken ct = default)
    {
        var job = await repo.GetByIdAsync(cmd.ImportJobId, ct);
        if (job is null)
            return ValidationResult<int>.NotFound($"Import job #{cmd.ImportJobId} không tồn tại.");

        if (job.Status != ImportJobStatus.Validated)
            return ValidationResult<int>.Failure(
                $"Import job #{cmd.ImportJobId} đã được thực hiện (trạng thái: {job.Status}).");

        if (string.IsNullOrEmpty(job.ValidRowsJson) || job.ValidRows == 0)
        {
            job.MarkCompleted(0);
            await repo.SaveChangesAsync(ct);
            return ValidationResult<int>.Ok(0);
        }

        try
        {
            var committed = await importService.ExecuteAsync(job.Category, job.ValidRowsJson, ct);
            job.MarkCompleted(committed);
            await repo.SaveChangesAsync(ct);
            return ValidationResult<int>.Ok(committed);
        }
        catch (Exception ex)
        {
            job.MarkFailed(ex.Message);
            await repo.SaveChangesAsync(ct);
            return ValidationResult<int>.Failure($"Lỗi khi nhập khẩu dữ liệu: {ex.Message}");
        }
    }
}
