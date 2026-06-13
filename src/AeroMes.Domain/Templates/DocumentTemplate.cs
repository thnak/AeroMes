using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Templates;

public enum DocumentType
{
    ProductionOrder,
    QualityInspectionForm,
    MaterialStandard,
    MaterialStandardStructure
}

public enum PrintOutputFormat
{
    Excel,
    Pdf
}

public class DocumentTemplate : AuditableEntity
{
    public int TemplateId { get; private set; }
    public string TemplateName { get; private set; } = string.Empty;
    public DocumentType DocumentType { get; private set; }
    public PrintOutputFormat OutputFormat { get; private set; }
    public Guid FileId { get; private set; }
    public bool IsActive { get; private set; } = true;

    private DocumentTemplate() { }

    public static DocumentTemplate Create(
        string templateName,
        DocumentType documentType,
        PrintOutputFormat outputFormat,
        Guid fileId,
        string? createdBy)
    {
        if (string.IsNullOrWhiteSpace(templateName))
            throw new DomainException("Tên mẫu in không được để trống.");

        return new DocumentTemplate
        {
            TemplateName = templateName.Trim(),
            DocumentType = documentType,
            OutputFormat = outputFormat,
            FileId = fileId,
            IsActive = true,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string templateName, PrintOutputFormat outputFormat, bool isActive, string? updatedBy)
    {
        if (string.IsNullOrWhiteSpace(templateName))
            throw new DomainException("Tên mẫu in không được để trống.");

        TemplateName = templateName.Trim();
        OutputFormat = outputFormat;
        IsActive = isActive;
        Touch(updatedBy);
    }
}
