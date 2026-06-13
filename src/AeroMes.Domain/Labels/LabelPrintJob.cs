namespace AeroMes.Domain.Labels;

public sealed class LabelPrintJob
{
    public Guid Id { get; private set; }
    public Guid TemplateId { get; private set; }
    public string PrintScope { get; private set; } = "ByOrder";
    public string EntityType { get; private set; } = "";
    public string EntityId { get; private set; } = "";
    public string? EntityCode { get; private set; }
    public int Quantity { get; private set; } = 1;
    public string Status { get; private set; } = "PENDING";
    public string PrintedBy { get; private set; } = "";
    public DateTime CreatedAt { get; private set; }

    private LabelPrintJob() { }

    public static LabelPrintJob Create(
        Guid templateId,
        string printScope,
        string entityType,
        string entityId,
        string? entityCode,
        int quantity,
        string printedBy)
    {
        return new LabelPrintJob
        {
            Id = Guid.NewGuid(),
            TemplateId = templateId,
            PrintScope = printScope,
            EntityType = entityType,
            EntityId = entityId,
            EntityCode = entityCode,
            Quantity = quantity < 1 ? 1 : quantity,
            Status = "PENDING",
            PrintedBy = printedBy,
            CreatedAt = DateTime.UtcNow,
        };
    }

    public void MarkPrinted() => Status = "PRINTED";
}
