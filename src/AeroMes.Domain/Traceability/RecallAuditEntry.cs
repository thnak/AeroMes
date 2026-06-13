using AeroMes.Domain.Common;

namespace AeroMes.Domain.Traceability;

public class RecallAuditEntry : Entity
{
    public long AuditID { get; private set; }
    public Guid RecallID { get; private set; }
    public string ActionType { get; private set; } = string.Empty;
    public string? ActionDetail { get; private set; }
    public string PerformedBy { get; private set; } = string.Empty;
    public DateTime PerformedAt { get; private set; }
    public bool SystemGenerated { get; private set; }

    private RecallAuditEntry() { }

    public static RecallAuditEntry Log(
        Guid recallId,
        string actionType,
        string performedBy,
        string? actionDetail = null,
        bool systemGenerated = false)
    {
        return new RecallAuditEntry
        {
            RecallID = recallId,
            ActionType = actionType.Trim(),
            ActionDetail = actionDetail?.Trim(),
            PerformedBy = performedBy.Trim(),
            PerformedAt = DateTime.UtcNow,
            SystemGenerated = systemGenerated,
        };
    }
}
