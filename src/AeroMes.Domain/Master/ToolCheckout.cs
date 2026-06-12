using AeroMes.Domain.Common;

namespace AeroMes.Domain.Master;

/// <summary>Sign-out / return log: tool room → work center.</summary>
public class ToolCheckout : Entity
{
    public long CheckoutId { get; private set; }
    public int ToolId { get; private set; }
    public int WorkCenterId { get; private set; }
    public string CheckedOutBy { get; private set; } = string.Empty;
    public DateTime CheckedOutAt { get; private set; }
    public DateTime? ExpectedReturnAt { get; private set; }
    public DateTime? ReturnedAt { get; private set; }
    public string? ReturnedBy { get; private set; }
    public ToolReturnCondition? ConditionOnReturn { get; private set; }
    public string? Notes { get; private set; }

    // EF navigation
    public WorkCenter? WorkCenter { get; private set; }

    private ToolCheckout() { }

    internal static ToolCheckout Create(
        int toolId, int workCenterId, string checkedOutBy, DateTime? expectedReturnAt)
    {
        return new ToolCheckout
        {
            ToolId = toolId,
            WorkCenterId = workCenterId,
            CheckedOutBy = checkedOutBy.Trim(),
            CheckedOutAt = DateTime.UtcNow,
            ExpectedReturnAt = expectedReturnAt,
        };
    }

    internal void Close(string returnedBy, ToolReturnCondition condition, string? notes)
    {
        ReturnedAt = DateTime.UtcNow;
        ReturnedBy = returnedBy.Trim();
        ConditionOnReturn = condition;
        Notes = notes;
    }
}
