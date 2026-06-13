using AeroMes.Domain.Common;

namespace AeroMes.Domain.Energy;

public class ShiftConsumption : Entity
{
    public long ConsumptionID { get; private set; }
    public int MeterID { get; private set; }
    public string ShiftCode { get; private set; } = string.Empty;
    public DateOnly ShiftDate { get; private set; }
    public long StartReadingID { get; private set; }
    public long? EndReadingID { get; private set; }
    public decimal? ConsumedUnits { get; private set; }
    public decimal? EnergyCost { get; private set; }
    public int? QtyProduced { get; private set; }
    public decimal? EnergyIntensity { get; private set; }
    public int? WOID { get; private set; }

    private ShiftConsumption() { }

    public static ShiftConsumption OpenShift(int meterId, string shiftCode, DateOnly shiftDate, long startReadingId, int? woid)
        => new()
        {
            MeterID = meterId,
            ShiftCode = shiftCode.Trim(),
            ShiftDate = shiftDate,
            StartReadingID = startReadingId,
            WOID = woid,
        };

    public void Close(long endReadingId, decimal endValue, decimal startValue, decimal? tariffRate, int? qtyProduced)
    {
        EndReadingID = endReadingId;
        ConsumedUnits = endValue - startValue;
        if (ConsumedUnits < 0) ConsumedUnits = 0;
        EnergyCost = tariffRate.HasValue ? ConsumedUnits * tariffRate.Value : null;
        QtyProduced = qtyProduced;
        EnergyIntensity = (EnergyCost.HasValue && qtyProduced > 0)
            ? EnergyCost / qtyProduced
            : null;
    }
}
