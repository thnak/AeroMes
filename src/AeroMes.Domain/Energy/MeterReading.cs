using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Energy;

public enum ReadingType { ShiftStart, ShiftEnd, Manual, AutoOpc }

public class MeterReading : Entity
{
    public long ReadingID { get; private set; }
    public int MeterID { get; private set; }
    public ReadingType ReadingType { get; private set; }
    public decimal ReadingValue { get; private set; }
    public DateTime ReadingAt { get; private set; }
    public string? ShiftCode { get; private set; }
    public string? EnteredBy { get; private set; }
    public string? Notes { get; private set; }

    private MeterReading() { }

    public static MeterReading Create(
        int meterId, ReadingType readingType, decimal readingValue,
        DateTime readingAt, string? shiftCode, string? enteredBy, string? notes)
    {
        if (readingValue < 0) throw new DomainException("Giá trị đọc không thể âm.");
        return new MeterReading
        {
            MeterID = meterId,
            ReadingType = readingType,
            ReadingValue = readingValue,
            ReadingAt = readingAt,
            ShiftCode = shiftCode?.Trim(),
            EnteredBy = enteredBy?.Trim(),
            Notes = notes?.Trim(),
        };
    }
}
