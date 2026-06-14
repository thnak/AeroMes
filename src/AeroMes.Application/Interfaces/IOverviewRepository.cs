namespace AeroMes.Application.Interfaces;

public record IncompleteOrdersResult(
    int TotalWOs, int IncompleteWOs, double CompletionPct,
    int TotalPOs, int IncompletePOs);

public record RemainingVolumeItem(
    string ProductCode, string? ProductName,
    int PlannedQty, int ActualQtyOK, int RemainingQty);

public record OutputOverTimeItem(string Period, int TotalQtyOK);

public record OrdersByStatusItem(string Status, int Count);

public record OutputByStageItem(string OperationCode, string OperationName, int TotalQtyOK);

public record OutputByDepartmentItem(string WorkCenterCode, string WorkCenterName, int TotalQtyOK);

public record TopProductByVolumeItem(string ProductCode, string? ProductName, int TotalQtyOK);

public record TopProductByErrorRateItem(
    string ProductCode, string? ProductName,
    int TotalQtyOK, decimal TotalDefects, double ErrorRate);

public record ErrorRateByCategoryItem(string Category, decimal TotalDefects, double Percentage);

public record StoppageReasonItem(string ReasonCode, string? ReasonName, int Count, double TotalHours);

// ── Dashboard KPI DTOs ────────────────────────────────────────────────────────

public record FactoryKpiDto(
    int ActiveWorkOrders,
    int TotalTargetQty,
    int TotalActualQtyOK,
    int TotalActualQtyNG,
    double OutputAchievementPct,
    int OpenDowntimeCount,
    double TotalDowntimeMinutesToday,
    int LowStockAlertCount,
    int ExpiringLotCount);

public record OeeByMachineDto(
    string MachineCode,
    double AvailabilityPct,
    double PerformancePct,
    double QualityPct,
    double OeePct);

public record ShiftOutputDto(
    string ShiftCode,
    int QtyOK,
    int QtyNG,
    double QualityRatePct,
    int ActiveJobCount,
    int FinishedJobCount);

public record DefectParetoItemDto(
    string DefectCode,
    string DefectName,
    string Category,
    string Severity,
    int Count,
    double CumulativePct);

public record LowStockAlertDto(string ProductCode, decimal AvailableQty, string LocationCode);
public record ExpiringLotAlertDto(string ProductCode, string LotNumber, DateOnly ExpiryDate, decimal Qty, int DaysUntilExpiry);
public record InventoryAlertSummaryDto(
    IReadOnlyList<LowStockAlertDto> LowStock,
    IReadOnlyList<ExpiringLotAlertDto> ExpiringLots);

public record OverdueSoDto(string SOCode, string? CustomerName, DateTime DeliveryDate, int OverdueDays);
public record SoFulfillmentDto(
    int TotalOrders,
    int OnTimeCount,
    int OverdueCount,
    int InProductionCount,
    double OnTimeRatePct,
    IReadOnlyList<OverdueSoDto> TopOverdue);

public record MyJobSummaryDto(long JobId, string WoCode, string MachineCode, string ShiftCode,
    int QtyOK, int QtyNG, DateTime StartTime, DateTime? EndTime);
public record MyDailyOutputDto(
    string OperatorId,
    int QtyOK,
    int QtyNG,
    double QualityRatePct,
    int JobsFinished,
    int JobsActive,
    double TotalDowntimeMinutes,
    IReadOnlyList<MyJobSummaryDto> Jobs);

public interface IOverviewRepository
{
    Task<IncompleteOrdersResult> GetIncompleteOrdersAsync(DateTime? from, DateTime? to, CancellationToken ct);
    Task<IReadOnlyList<RemainingVolumeItem>> GetRemainingVolumeAsync(DateTime? from, DateTime? to, CancellationToken ct);
    Task<IReadOnlyList<OutputOverTimeItem>> GetOutputOverTimeAsync(DateTime from, DateTime to, string granularity, CancellationToken ct);
    Task<IReadOnlyList<OrdersByStatusItem>> GetOrdersByStatusAsync(DateTime? from, DateTime? to, CancellationToken ct);
    Task<IReadOnlyList<OutputByStageItem>> GetOutputByStageAsync(DateTime? from, DateTime? to, CancellationToken ct);
    Task<IReadOnlyList<OutputByDepartmentItem>> GetOutputByDepartmentAsync(DateTime? from, DateTime? to, CancellationToken ct);
    Task<IReadOnlyList<TopProductByVolumeItem>> GetTopProductsByVolumeAsync(DateTime? from, DateTime? to, int topN, CancellationToken ct);
    Task<IReadOnlyList<TopProductByErrorRateItem>> GetTopProductsByErrorRateAsync(DateTime? from, DateTime? to, int topN, CancellationToken ct);
    Task<IReadOnlyList<ErrorRateByCategoryItem>> GetErrorRateByCategoryAsync(DateTime? from, DateTime? to, CancellationToken ct);
    Task<IReadOnlyList<StoppageReasonItem>> GetStoppageReasonsAsync(DateTime? from, DateTime? to, CancellationToken ct);

    // Dashboard queries
    Task<FactoryKpiDto> GetFactoryKpiAsync(DateOnly date, CancellationToken ct);
    Task<IReadOnlyList<OeeByMachineDto>> GetOeeByMachineAsync(DateOnly from, DateOnly to, CancellationToken ct);
    Task<IReadOnlyList<ShiftOutputDto>> GetShiftOutputSummaryAsync(DateOnly date, CancellationToken ct);
    Task<IReadOnlyList<DefectParetoItemDto>> GetDefectParetoAsync(DateOnly from, DateOnly to, string? productCode, CancellationToken ct);
    Task<InventoryAlertSummaryDto> GetInventoryAlertsAsync(CancellationToken ct);
    Task<SoFulfillmentDto> GetSoFulfillmentRateAsync(int year, int month, CancellationToken ct);
    Task<MyDailyOutputDto> GetMyDailyOutputAsync(string operatorId, DateOnly date, CancellationToken ct);
    Task<IReadOnlyList<MyDailyOutputDto>> GetMyOutputHistoryAsync(string operatorId, int days, CancellationToken ct);
}
