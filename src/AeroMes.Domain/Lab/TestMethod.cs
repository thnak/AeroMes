namespace AeroMes.Domain.Lab;

public class TestMethod
{
    public int TestMethodId { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Category { get; private set; } = string.Empty;
    public string Unit { get; private set; } = string.Empty;
    public string MeasurementType { get; private set; } = "VARIABLE";
    public decimal? SpecMin { get; private set; }
    public decimal? SpecMax { get; private set; }
    public decimal? SpecNominal { get; private set; }
    public string? ReferenceStd { get; private set; }
    public string? InstrumentType { get; private set; }
    public int? EstDurationMin { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private TestMethod() { }

    public static TestMethod Create(string code, string name, string category, string unit,
        string measurementType, decimal? specMin, decimal? specMax, decimal? specNominal,
        string? referenceStd, string? instrumentType, int? estDurationMin)
        => new()
        {
            Code = code, Name = name, Category = category, Unit = unit,
            MeasurementType = measurementType, SpecMin = specMin, SpecMax = specMax,
            SpecNominal = specNominal, ReferenceStd = referenceStd,
            InstrumentType = instrumentType, EstDurationMin = estDurationMin,
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
        };

    public void Update(string name, string category, string unit, string measurementType,
        decimal? specMin, decimal? specMax, decimal? specNominal,
        string? referenceStd, string? instrumentType, int? estDurationMin)
    {
        Name = name; Category = category; Unit = unit; MeasurementType = measurementType;
        SpecMin = specMin; SpecMax = specMax; SpecNominal = specNominal;
        ReferenceStd = referenceStd; InstrumentType = instrumentType;
        EstDurationMin = estDurationMin; UpdatedAt = DateTime.UtcNow;
    }

    public void Toggle(bool active) { IsActive = active; UpdatedAt = DateTime.UtcNow; }
}
