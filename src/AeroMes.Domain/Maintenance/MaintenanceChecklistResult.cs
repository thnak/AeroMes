using AeroMes.Domain.Common;

namespace AeroMes.Domain.Maintenance;

public class MaintenanceChecklistResult : Entity
{
    public long ResultId { get; private set; }
    public int MwoId { get; private set; }
    public int ItemId { get; private set; }
    public bool IsCompleted { get; private set; }
    public string? ObservationNotes { get; private set; }
    public string? CompletedBy { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    public MaintenanceChecklistItem? Item { get; private set; }

    private MaintenanceChecklistResult() { }

    public static MaintenanceChecklistResult Create(
        int mwoId, int itemId, bool isCompleted,
        string? observationNotes, string? completedBy)
        => new()
        {
            MwoId = mwoId,
            ItemId = itemId,
            IsCompleted = isCompleted,
            ObservationNotes = observationNotes?.Trim(),
            CompletedBy = completedBy?.Trim(),
            CompletedAt = isCompleted ? DateTime.UtcNow : null,
        };
}
