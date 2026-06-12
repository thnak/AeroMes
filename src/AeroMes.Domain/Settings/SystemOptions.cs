namespace AeroMes.Domain.Settings;

public class SystemOptions
{
    public int Id { get; private set; } = 1; // singleton row

    // ── Purchase Orders ──────────────────────────────────────────────────
    public bool PurchaseOrderRetrievalPrevention { get; set; }
    public bool PurchaseOrderAutoGenerateProductionPlan { get; set; }
    public string? PurchaseOrderDefaultAllocationMethod { get; set; }

    // ── Materials & Goods ────────────────────────────────────────────────
    public string MaterialManagementType { get; set; } = "None"; // None | VariantCode | SpecificationCode
    public bool MaterialAutoGenerateWarehouseDocs { get; set; }
    public bool MaterialTrackExcessAndByProducts { get; set; }
    public bool MaterialByProductTracking { get; set; }
    public bool MaterialBatchAndExpiryManagement { get; set; }
    public bool MaterialDimensionTracking { get; set; }
    public bool MaterialUnitConversionEditable { get; set; } = true;
    public bool MaterialForecastStockWarning { get; set; }
    public bool MaterialDefectRateManagement { get; set; }

    // ── Production Capacity ──────────────────────────────────────────────
    public bool CapacityMoldToolingManagement { get; set; }

    // ── Dispatch & Execution — Production Orders ─────────────────────────
    public bool DispatchAutoGenerateSubAssemblyOrders { get; set; }
    public bool DispatchAutoStatusTransition { get; set; }
    public string DispatchSequentialWorkflowEnforcement { get; set; } = "Off"; // Off | Warn | Block

    // ── Dispatch & Execution — Material Requests ─────────────────────────
    public bool DispatchAutoGenerateSupplyRequests { get; set; }

    // ── Dispatch & Execution — Production Reporting ───────────────────────
    public bool ReportingOverageQuantityAlert { get; set; }
    public bool ReportingEditLockAfterQcRequest { get; set; }
    public bool ReportingAutoGenerateFromCompletedWorkOrders { get; set; }
    public bool ReportingPrintLimitEnforcement { get; set; }

    // ── Semi-Product Handoffs ─────────────────────────────────────────────
    public bool HandoffTrackInterStageTransfers { get; set; }
    public bool HandoffAutoConfirmation { get; set; }

    // ── Finished Goods Acceptance ─────────────────────────────────────────
    public bool AcceptanceAutoGenerateFromProductionReports { get; set; }
    public bool AcceptanceQuantityCategorization { get; set; }

    // ── Quality Control ───────────────────────────────────────────────────
    public bool QcAutoGenerateRequestsAfterReporting { get; set; }
    public string QcTargetSelection { get; set; } = "Both"; // FinishedGoods | ProcessStages | Both

    // ── Audit ─────────────────────────────────────────────────────────────
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string? UpdatedBy { get; set; }

    // Public for System.Text.Json model binding (PUT /settings/system-options).
    public SystemOptions() { }

    public static SystemOptions CreateDefaults() => new()
    {
        Id = 1,
        MaterialManagementType = "None",
        MaterialUnitConversionEditable = true,
        DispatchSequentialWorkflowEnforcement = "Off",
        QcTargetSelection = "Both",
        UpdatedAt = DateTime.UtcNow,
    };

    public void Update(SystemOptions patch, string? updatedBy)
    {
        PurchaseOrderRetrievalPrevention = patch.PurchaseOrderRetrievalPrevention;
        PurchaseOrderAutoGenerateProductionPlan = patch.PurchaseOrderAutoGenerateProductionPlan;
        PurchaseOrderDefaultAllocationMethod = patch.PurchaseOrderDefaultAllocationMethod;
        MaterialManagementType = patch.MaterialManagementType;
        MaterialAutoGenerateWarehouseDocs = patch.MaterialAutoGenerateWarehouseDocs;
        MaterialTrackExcessAndByProducts = patch.MaterialTrackExcessAndByProducts;
        MaterialByProductTracking = patch.MaterialByProductTracking;
        MaterialBatchAndExpiryManagement = patch.MaterialBatchAndExpiryManagement;
        MaterialDimensionTracking = patch.MaterialDimensionTracking;
        MaterialUnitConversionEditable = patch.MaterialUnitConversionEditable;
        MaterialForecastStockWarning = patch.MaterialForecastStockWarning;
        MaterialDefectRateManagement = patch.MaterialDefectRateManagement;
        CapacityMoldToolingManagement = patch.CapacityMoldToolingManagement;
        DispatchAutoGenerateSubAssemblyOrders = patch.DispatchAutoGenerateSubAssemblyOrders;
        DispatchAutoStatusTransition = patch.DispatchAutoStatusTransition;
        DispatchSequentialWorkflowEnforcement = patch.DispatchSequentialWorkflowEnforcement;
        DispatchAutoGenerateSupplyRequests = patch.DispatchAutoGenerateSupplyRequests;
        ReportingOverageQuantityAlert = patch.ReportingOverageQuantityAlert;
        ReportingEditLockAfterQcRequest = patch.ReportingEditLockAfterQcRequest;
        ReportingAutoGenerateFromCompletedWorkOrders = patch.ReportingAutoGenerateFromCompletedWorkOrders;
        ReportingPrintLimitEnforcement = patch.ReportingPrintLimitEnforcement;
        HandoffTrackInterStageTransfers = patch.HandoffTrackInterStageTransfers;
        HandoffAutoConfirmation = patch.HandoffAutoConfirmation;
        AcceptanceAutoGenerateFromProductionReports = patch.AcceptanceAutoGenerateFromProductionReports;
        AcceptanceQuantityCategorization = patch.AcceptanceQuantityCategorization;
        QcAutoGenerateRequestsAfterReporting = patch.QcAutoGenerateRequestsAfterReporting;
        QcTargetSelection = patch.QcTargetSelection;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }
}
