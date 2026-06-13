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
}
