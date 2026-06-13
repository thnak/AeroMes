using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Quality;

public class InspectionCharacteristic
{
    public int CharId { get; private set; }
    public int PlanId { get; private set; }
    public int Sequence { get; private set; }
    public string CharName { get; private set; } = string.Empty;
    public string MeasurementType { get; private set; } = string.Empty; // VARIABLE | ATTRIBUTE
    public decimal? SpecMin { get; private set; }
    public decimal? SpecMax { get; private set; }
    public decimal? SpecNominal { get; private set; }
    public string? Unit { get; private set; }
    public string? AttributeSpec { get; private set; }
    public bool IsRequired { get; private set; } = true;
    public string SeverityLevel { get; private set; } = string.Empty; // CRITICAL | MAJOR | MINOR
    public string? DefectCodeLink { get; private set; }
    public string? Notes { get; private set; }

    // EF navigation
    public InspectionPlan? Plan { get; private set; }

    private InspectionCharacteristic() { }

    public static InspectionCharacteristic Create(
        int planId,
        int sequence,
        string charName,
        string measurementType,
        decimal? specMin,
        decimal? specMax,
        decimal? specNominal,
        string? unit,
        string? attributeSpec,
        bool isRequired,
        string severityLevel,
        string? defectCodeLink,
        string? notes)
    {
        if (string.IsNullOrWhiteSpace(charName))
            throw new DomainException("Characteristic name is required.");
        if (string.IsNullOrWhiteSpace(measurementType))
            throw new DomainException("Measurement type is required.");
        if (sequence <= 0)
            throw new DomainException("Sequence must be a positive integer.");

        return new InspectionCharacteristic
        {
            PlanId = planId,
            Sequence = sequence,
            CharName = charName.Trim(),
            MeasurementType = measurementType.Trim().ToUpperInvariant(),
            SpecMin = specMin,
            SpecMax = specMax,
            SpecNominal = specNominal,
            Unit = unit?.Trim(),
            AttributeSpec = attributeSpec?.Trim(),
            IsRequired = isRequired,
            SeverityLevel = severityLevel.Trim().ToUpperInvariant(),
            DefectCodeLink = defectCodeLink?.Trim().ToUpperInvariant(),
            Notes = notes?.Trim(),
        };
    }

    public void Update(
        int sequence,
        string charName,
        string measurementType,
        decimal? specMin,
        decimal? specMax,
        decimal? specNominal,
        string? unit,
        string? attributeSpec,
        bool isRequired,
        string severityLevel,
        string? defectCodeLink,
        string? notes)
    {
        if (string.IsNullOrWhiteSpace(charName))
            throw new DomainException("Characteristic name is required.");
        if (sequence <= 0)
            throw new DomainException("Sequence must be a positive integer.");

        Sequence = sequence;
        CharName = charName.Trim();
        MeasurementType = measurementType.Trim().ToUpperInvariant();
        SpecMin = specMin;
        SpecMax = specMax;
        SpecNominal = specNominal;
        Unit = unit?.Trim();
        AttributeSpec = attributeSpec?.Trim();
        IsRequired = isRequired;
        SeverityLevel = severityLevel.Trim().ToUpperInvariant();
        DefectCodeLink = defectCodeLink?.Trim().ToUpperInvariant();
        Notes = notes?.Trim();
    }

    public void Resequence(int newSeq)
    {
        if (newSeq <= 0)
            throw new DomainException("Sequence must be a positive integer.");
        Sequence = newSeq;
    }
}
