using AeroMes.Application.Common;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Production.Queries.GetProductionOrderProgress;

public sealed record GetProductionOrderProgressQuery(int POID)
    : IQuery<QueryResult<ProductionOrderProgressDto>>;

// ── Top-level DTO ────────────────────────────────────────────────────────────

public sealed record ProductionOrderProgressDto(
    int POID,
    string POCode,
    string ProductCode,
    int TargetQuantity,
    string Status,
    DateTime? PlannedStart,
    DateTime? PlannedEnd,
    // KPIs
    int TotalQtyOK,
    int TotalQtyNG,
    double OverallProgressPct,           // TotalQtyOK / TargetQuantity × 100
    double PlannedHours,
    double UsedHours,
    double TimeUsagePct,
    double QualityPassRate,              // QtyOK / (QtyOK + QtyNG) × 100
    // Per-tab views
    IReadOnlyList<StageProgressDto> Stages,
    IReadOnlyList<MaterialProgressDto> Materials,
    IReadOnlyList<OutputSummaryDto> OutputSummaries,
    IReadOnlyList<QualityProgressDto> QualityItems,
    IReadOnlyList<HandoverProgressDto> Handovers);

// ── Stages ───────────────────────────────────────────────────────────────────

public sealed record StageProgressDto(
    int WOID,
    string WOCode,
    int StepNumber,
    string OperationCode,
    string WorkCenterName,
    string Status,
    int TargetQty,
    int ActualQtyOK,
    int ActualQtyNG,
    int NotYetProducedQty,   // TargetQty - ActualQtyOK - ActualQtyNG
    DateTime? ActualStart,
    DateTime? ActualEnd,
    int JobCount,
    bool HasHandoverPending);

// ── Materials ────────────────────────────────────────────────────────────────

public sealed record MaterialProgressDto(
    string ComponentCode,
    string ComponentName,
    decimal RequiredQtyTotal,
    string UoMCode);

// ── Output ───────────────────────────────────────────────────────────────────

public sealed record OutputSummaryDto(
    int WOID,
    string WOCode,
    string OperationCode,
    long JobID,
    string MachineCode,
    string ShiftCode,
    DateTime JobStart,
    DateTime? JobEnd,
    int QtyOK,
    int QtyNG);

// ── Quality ──────────────────────────────────────────────────────────────────

public sealed record QualityProgressDto(
    int WOID,
    string WOCode,
    string OperationCode,
    int TotalDefects,
    int DefectVarieties,
    IReadOnlyList<DefectSummaryDto> DefectBreakdown);

public sealed record DefectSummaryDto(int DefectCodeId, int TotalQty);

// ── Handover ─────────────────────────────────────────────────────────────────

public sealed record HandoverProgressDto(
    int FormID,
    string FormNumber,
    string FormType,
    string Status,
    int FromWOID,
    int ToWOID,
    DateTime? HandoverDate);
