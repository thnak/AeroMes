namespace AeroMes.Application.Import;

public record ImportRowError(int RowNumber, string[] Errors);
public record ImportValidationResult(
    string Category,
    string FileName,
    int TotalRows,
    int ValidRows,
    int InvalidRows,
    IReadOnlyList<ImportRowError> Errors,
    string? ValidRowsJson,
    string? ErrorRowsJson);

public interface IImportService
{
    // Returns the list of supported categories with their display names and required columns.
    IReadOnlyList<ImportCategoryInfo> GetSupportedCategories();

    // Generates an Excel template file for the given category.
    Task<byte[]> GetTemplateAsync(string category, CancellationToken ct);

    // Parses and validates the uploaded file; returns per-row results.
    Task<ImportValidationResult> ValidateAsync(
        Stream fileStream,
        string fileName,
        string category,
        int startRow,
        CancellationToken ct);

    // Commits valid rows from a previously validated job's JSON payload.
    Task<int> ExecuteAsync(string category, string validRowsJson, CancellationToken ct);

    // Generates an Excel error-report for the given error rows JSON.
    Task<byte[]> GetErrorReportAsync(string errorRowsJson, CancellationToken ct);
}

public record ImportColumnDef(string Name, string Description, bool IsRequired);
public record ImportCategoryInfo(string Category, string DisplayName, IReadOnlyList<ImportColumnDef> Columns);
