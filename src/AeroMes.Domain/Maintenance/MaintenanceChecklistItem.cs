using AeroMes.Domain.Common;

namespace AeroMes.Domain.Maintenance;

public class MaintenanceChecklistItem : Entity
{
    public int ItemId { get; private set; }
    public int TemplateId { get; private set; }
    public int StepOrder { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public bool RequiresPartReplacement { get; private set; }
    public string? PartCode { get; private set; }

    public MaintenancePlanTemplate? Template { get; private set; }

    private MaintenanceChecklistItem() { }

    public static MaintenanceChecklistItem Create(
        int templateId, int stepOrder, string description,
        bool requiresPartReplacement = false, string? partCode = null)
        => new()
        {
            TemplateId = templateId,
            StepOrder = stepOrder,
            Description = description.Trim(),
            RequiresPartReplacement = requiresPartReplacement,
            PartCode = partCode?.Trim().ToUpperInvariant(),
        };
}
