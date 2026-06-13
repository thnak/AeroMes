using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Wms;

public class IntakeRequestLine : Entity
{
    public int LineId { get; private set; }
    public int IntakeRequestId { get; private set; }
    public string ProductCode { get; private set; } = string.Empty;
    public string UnitOfMeasure { get; private set; } = string.Empty;
    public decimal RequestedQuantity { get; private set; }
    public int WarehouseId { get; private set; }
    public bool IsDefective { get; private set; }
    public string? DefectReason { get; private set; }
    public decimal? ActualReceivedQuantity { get; private set; }
    public string? Notes { get; private set; }

    private IntakeRequestLine() { }

    internal static IntakeRequestLine Create(
        int intakeRequestId,
        string productCode,
        string unitOfMeasure,
        decimal requestedQuantity,
        int warehouseId,
        bool isDefective,
        string? defectReason,
        string? notes)
    {
        return new IntakeRequestLine
        {
            IntakeRequestId = intakeRequestId,
            ProductCode = productCode.Trim().ToUpperInvariant(),
            UnitOfMeasure = unitOfMeasure.Trim(),
            RequestedQuantity = requestedQuantity,
            WarehouseId = warehouseId,
            IsDefective = isDefective,
            DefectReason = defectReason?.Trim(),
            Notes = notes?.Trim(),
        };
    }

    internal void SetActualReceivedQuantity(decimal qty)
    {
        if (qty < 0)
            throw new DomainException("Số lượng thực nhập không được âm.");
        ActualReceivedQuantity = qty;
    }
}
