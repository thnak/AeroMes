using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Production;

public class ProductionLog : Entity
{
    public long LogID { get; private set; }
    public int WorkOrderID { get; private set; }
    public DateTime Timestamp { get; private set; }
    public int QtyOK { get; private set; }
    public int QtyNG { get; private set; }
    public string OperatorID { get; private set; } = string.Empty;
    public string? MachineCode { get; private set; }
    public string? ShiftCode { get; private set; }
    public string? Notes { get; private set; }
    public string? IdempotencyKey { get; private set; }

    private readonly List<DefectDetail> _defectDetails = [];
    public IReadOnlyCollection<DefectDetail> DefectDetails => _defectDetails.AsReadOnly();

    private ProductionLog() { } // EF constructor

    public static ProductionLog Create(
        int workOrderId,
        int qtyOk,
        int qtyNg,
        string operatorId,
        string? machineCode,
        string? shiftCode,
        string? idempotencyKey,
        DateTime? timestamp = null)
    {
        return new ProductionLog
        {
            WorkOrderID = workOrderId,
            QtyOK = qtyOk,
            QtyNG = qtyNg,
            OperatorID = operatorId,
            MachineCode = machineCode,
            ShiftCode = shiftCode,
            IdempotencyKey = idempotencyKey,
            Timestamp = timestamp ?? DateTime.UtcNow,
        };
    }

    public void AddDefect(int defectCodeId, int quantity)
    {
        if (quantity <= 0)
            throw new DomainException($"Defect quantity must be positive. Got: {quantity}.");
        _defectDetails.Add(new DefectDetail(defectCodeId, quantity));
    }
}
