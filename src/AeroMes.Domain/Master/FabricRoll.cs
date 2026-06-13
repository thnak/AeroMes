using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Master;

public enum FabricRollStatus { Available, Reserved, InUse, Exhausted, Quarantine, Returned }

public class FabricRoll : Entity
{
    public int RollID { get; private set; }
    public string RollBarcode { get; private set; } = string.Empty;
    public string FabricProductCode { get; private set; } = string.Empty;
    public string LotNumber { get; private set; } = string.Empty;
    public string ShadeCode { get; private set; } = string.Empty;
    public decimal GrossLengthMeters { get; private set; }
    public decimal GrossWeightKg { get; private set; }
    public decimal RemainingLengthMeters { get; private set; }
    public decimal RemainingWeightKg { get; private set; }  // SQL computed column
    public decimal FabricWidthCm { get; private set; }
    public string? SupplierCode { get; private set; }
    public DateOnly ReceivedDate { get; private set; }
    public int? LocationID { get; private set; }
    public FabricRollStatus Status { get; private set; } = FabricRollStatus.Available;
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAt { get; private set; }

    private FabricRoll() { }

    public static FabricRoll Register(
        string rollBarcode,
        string fabricProductCode,
        string lotNumber,
        string shadeCode,
        decimal grossLengthMeters,
        decimal grossWeightKg,
        decimal fabricWidthCm,
        string? supplierCode,
        int? locationId)
    {
        if (grossLengthMeters <= 0) throw new DomainException("Gross length must be positive.");
        if (grossWeightKg <= 0) throw new DomainException("Gross weight must be positive.");
        if (fabricWidthCm <= 0) throw new DomainException("Fabric width must be positive.");

        return new FabricRoll
        {
            RollBarcode = rollBarcode.Trim().ToUpperInvariant(),
            FabricProductCode = fabricProductCode.Trim().ToUpperInvariant(),
            LotNumber = lotNumber.Trim().ToUpperInvariant(),
            ShadeCode = shadeCode.Trim().ToUpperInvariant(),
            GrossLengthMeters = grossLengthMeters,
            GrossWeightKg = grossWeightKg,
            RemainingLengthMeters = grossLengthMeters,
            FabricWidthCm = fabricWidthCm,
            SupplierCode = supplierCode?.Trim().ToUpperInvariant(),
            ReceivedDate = DateOnly.FromDateTime(DateTime.UtcNow),
            LocationID = locationId,
            CreatedAt = DateTime.UtcNow,
        };
    }

    public void Reserve()
    {
        if (Status != FabricRollStatus.Available)
            throw new DomainException($"Roll '{RollBarcode}' is not available (current status: {Status}).");
        Status = FabricRollStatus.Reserved;
    }

    public decimal Consume(decimal metersConsumed)
    {
        if (metersConsumed <= 0) throw new DomainException("Meters consumed must be positive.");
        if (metersConsumed > RemainingLengthMeters)
            throw new DomainException(
                $"Cannot consume {metersConsumed}m from roll '{RollBarcode}' — only {RemainingLengthMeters}m remaining.");

        Status = FabricRollStatus.InUse;
        RemainingLengthMeters -= metersConsumed;
        if (RemainingLengthMeters < 0.5m) Status = FabricRollStatus.Exhausted;
        return RemainingLengthMeters;
    }

    public void Quarantine()
    {
        if (Status == FabricRollStatus.Exhausted)
            throw new DomainException("Cannot quarantine an exhausted roll.");
        Status = FabricRollStatus.Quarantine;
    }

    public void SetLocation(int? locationId) => LocationID = locationId;
}
