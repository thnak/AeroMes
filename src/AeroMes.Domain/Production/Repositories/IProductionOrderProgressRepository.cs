namespace AeroMes.Domain.Production.Repositories;

public sealed record PoProgressData(
    // Work orders (stages)
    IReadOnlyList<WoProgressRow> WorkOrders,
    // All jobs under those work orders
    IReadOnlyList<JobProgressRow> Jobs,
    // All production logs (output + defects)
    IReadOnlyList<LogProgressRow> Logs,
    // Handover forms linking any of these WOs
    IReadOnlyList<HandoverRow> Handovers);

public sealed record WoProgressRow(
    int WOID, string WOCode, int POID, int RoutingStepID,
    int StepNumber, string OperationCode, string WorkCenterName,
    string Status, int TargetQty, int ActualQtyOK, int ActualQtyNG,
    DateTime? ActualStart, DateTime? ActualEnd);

public sealed record JobProgressRow(
    long JobID, int WOID, string MachineCode, string ShiftCode,
    DateTime StartTime, DateTime? EndTime, string Status);

public sealed record LogProgressRow(
    long LogID, long JobID, int WOID, int QtyOK, int QtyNG,
    IReadOnlyList<(int DefectCodeId, int Qty)> Defects);

public sealed record HandoverRow(
    int FormID, string FormNumber, string FormType, string Status,
    int FromWOID, int ToWOID, DateTime? HandoverDate);

public interface IProductionOrderProgressRepository
{
    Task<PoProgressData?> GetProgressDataAsync(int poId, CancellationToken ct = default);
}
