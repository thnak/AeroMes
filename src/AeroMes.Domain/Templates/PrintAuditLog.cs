namespace AeroMes.Domain.Templates;

public class PrintAuditLog
{
    public int LogId { get; private set; }
    public string DocumentType { get; private set; } = string.Empty;
    public string DocumentId { get; private set; } = string.Empty;
    public int? TemplateId { get; private set; }
    public string TemplateName { get; private set; } = string.Empty;
    public string OutputFormat { get; private set; } = string.Empty;
    public string PrintedBy { get; private set; } = string.Empty;
    public DateTime PrintedAt { get; private set; }

    private PrintAuditLog() { }

    public static PrintAuditLog Create(
        string documentType,
        string documentId,
        int? templateId,
        string templateName,
        string outputFormat,
        string printedBy)
        => new()
        {
            DocumentType = documentType,
            DocumentId = documentId,
            TemplateId = templateId,
            TemplateName = templateName,
            OutputFormat = outputFormat,
            PrintedBy = printedBy,
            PrintedAt = DateTime.UtcNow
        };
}
