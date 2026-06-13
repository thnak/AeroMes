using AeroMes.Domain.Import;

namespace AeroMes.Application.Import;

public record ImportJobSummaryDto(
    int ImportJobId,
    string Category,
    string Status,
    string FileName,
    int TotalRows,
    int ValidRows,
    int InvalidRows,
    int CommittedRows,
    string? ErrorMessage,
    string? CreatedBy,
    DateTime CreatedAt,
    DateTime? CompletedAt);

public interface IImportRepository
{
    Task<int> AddAsync(ImportJob job, CancellationToken ct);
    Task<ImportJob?> GetByIdAsync(int id, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
    Task<IReadOnlyList<ImportJobSummaryDto>> GetHistoryAsync(int page, int pageSize, CancellationToken ct);
}
