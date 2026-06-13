namespace AeroMes.Application.Documents;

public record PrintDocumentResult(byte[] Content, string ContentType, string FileName);

public interface IDocumentPrintService
{
    // Renders a document using the system default PDF template.
    Task<PrintDocumentResult> RenderDefaultAsync(string documentType, string documentId, CancellationToken ct);

    // Renders a document using an uploaded custom Excel/PDF template.
    Task<PrintDocumentResult> RenderWithTemplateAsync(string documentType, string documentId, int templateId, CancellationToken ct);
}
