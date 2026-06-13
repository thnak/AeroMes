namespace AeroMes.Domain.Cost.Repositories;

public record WOCostSummaryDto(
    int WOCostID, int WOID, string WOCode,
    int? StdCostID, decimal StdTotalCost,
    decimal ActMaterialCost, decimal ActLaborCost,
    decimal ActMachineCost, decimal ActMaintenanceCost,
    decimal ActTotalCost, decimal TotalVariance,
    int ProducedQtyOK, int ScrapQty,
    string? VarianceDetailJson, DateTime UpdatedAt);

public record WOMaterialCostLineDto(
    long LineID, int WOID, long? ConsumptionID,
    string ProductCode, string? LotNumber,
    decimal QtyConsumed, decimal ActualUnitCost, decimal LineTotal, DateTime PostedAt);

public record WOLaborCostLineDto(
    long LineID, int WOID, long JobID,
    string OperatorID, int LaborGradeID, string GradeCode,
    decimal ActualHours, decimal HourlyRateSnapshot,
    bool IsOvertime, decimal OvertimeMultiplierSnapshot,
    decimal LineTotal, DateTime PostedAt);

public record WOMachineCostLineDto(
    long LineID, int WOID, long JobID,
    string MachineCode, decimal RuntimeHours, decimal? EnergyKWh,
    decimal TotalRateSnapshot, decimal LineTotal, DateTime PostedAt);

public record VarianceReportItemDto(
    int WOID, string WOCode, string ProductCode,
    decimal StdTotalCost, decimal ActTotalCost, decimal TotalVariance,
    decimal VariancePct, int ProducedQtyOK, DateTime UpdatedAt);

public interface IWOCostRepository
{
    Task<WOCostSummary?> GetSummaryByWOIDAsync(int woId, CancellationToken ct);
    Task AddSummaryAsync(WOCostSummary summary, CancellationToken ct);
    Task AddMaterialLineAsync(WOMaterialCostLine line, CancellationToken ct);
    Task AddLaborLineAsync(WOLaborCostLine line, CancellationToken ct);
    Task AddMachineLineAsync(WOMachineCostLine line, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);

    Task<WOCostSummaryDto?> GetSummaryDtoAsync(int woId, CancellationToken ct);
    Task<IReadOnlyList<WOMaterialCostLineDto>> GetMaterialLinesAsync(int woId, CancellationToken ct);
    Task<IReadOnlyList<WOLaborCostLineDto>> GetLaborLinesAsync(int woId, CancellationToken ct);
    Task<IReadOnlyList<WOMachineCostLineDto>> GetMachineLinesAsync(int woId, CancellationToken ct);

    Task<(IReadOnlyList<VarianceReportItemDto> Items, int Total)> GetVarianceReportAsync(
        string? productCode, DateOnly? from, DateOnly? to, int? workCenterId,
        int page, int pageSize, CancellationToken ct);
}
