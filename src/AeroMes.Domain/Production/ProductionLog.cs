using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Quality; // DefectDetail lives in qual schema

namespace AeroMes.Domain.Production;

public class ProductionLog : Entity
{
    public long LogID { get; private set; }
    public long JobID { get; private set; }
    public DateTime Timestamp { get; private set; }
    public int QtyOK { get; private set; }
    public int QtyNG { get; private set; }
    public string? DeviceIP { get; private set; }
    public string? Notes { get; private set; }
    public string? IdempotencyKey { get; private set; }

    private readonly List<DefectDetail> _defectDetails = [];
    public IReadOnlyCollection<DefectDetail> DefectDetails => _defectDetails.AsReadOnly();

    // EF navigation
    public Job? Job { get; private set; }

    private ProductionLog() { }

    public static ProductionLog Create(
        long jobId,
        int qtyOk,
        int qtyNg,
        string? deviceIp = null,
        string? idempotencyKey = null,
        string? notes = null,
        DateTime? timestamp = null)
    {
        if (qtyOk < 0) throw new DomainException($"QtyOK cannot be negative. Got: {qtyOk}.");
        if (qtyNg < 0) throw new DomainException($"QtyNG cannot be negative. Got: {qtyNg}.");

        return new ProductionLog
        {
            JobID = jobId,
            QtyOK = qtyOk,
            QtyNG = qtyNg,
            DeviceIP = deviceIp,
            Notes = notes,
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
