using AeroMes.Domain.Common;

namespace AeroMes.Domain.Import;

public enum ImportJobStatus { Validated, Executing, Completed, Failed }

public class ImportJob : AuditableEntity
{
    public int ImportJobId { get; private set; }
    public string Category { get; private set; } = string.Empty;
    public ImportJobStatus Status { get; private set; } = ImportJobStatus.Validated;
    public string FileName { get; private set; } = string.Empty;
    public int TotalRows { get; private set; }
    public int ValidRows { get; private set; }
    public int InvalidRows { get; private set; }
    public int CommittedRows { get; private set; }
    public string? ValidRowsJson { get; private set; }
    public string? ErrorRowsJson { get; private set; }
    public string? ErrorMessage { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    private ImportJob() { }

    public static ImportJob Create(
        string category,
        string fileName,
        int totalRows,
        int validRows,
        int invalidRows,
        string? validRowsJson,
        string? errorRowsJson,
        string? createdBy)
        => new()
        {
            Category = category,
            Status = ImportJobStatus.Validated,
            FileName = fileName,
            TotalRows = totalRows,
            ValidRows = validRows,
            InvalidRows = invalidRows,
            ValidRowsJson = validRowsJson,
            ErrorRowsJson = errorRowsJson,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        };

    public void MarkCompleted(int committedRows)
    {
        Status = ImportJobStatus.Completed;
        CommittedRows = committedRows;
        CompletedAt = DateTime.UtcNow;
    }

    public void MarkFailed(string error)
    {
        Status = ImportJobStatus.Failed;
        ErrorMessage = error;
        CompletedAt = DateTime.UtcNow;
    }
}
