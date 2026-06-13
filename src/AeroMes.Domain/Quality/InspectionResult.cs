using AeroMes.Domain.Common;

namespace AeroMes.Domain.Quality;

public class InspectionResult : Entity
{
    public long ResultId { get; private set; }
    public int InspectionOrderId { get; private set; }
    public int CharId { get; private set; }
    public decimal? MeasuredValue { get; private set; }
    public string? AttributeResult { get; private set; }  // PASS | FAIL
    public bool? IsWithinSpec { get; private set; }
    public int? DefectCodeId { get; private set; }
    public int? SampleIndex { get; private set; }
    public string? Notes { get; private set; }
    public string RecordedBy { get; private set; } = "";
    public DateTimeOffset RecordedAt { get; private set; }

    // Nav props
    public InspectionCharacteristic? Characteristic { get; private set; }

    private InspectionResult() { }

    public static InspectionResult Create(
        int inspectionOrderId,
        int charId,
        decimal? measuredValue,
        string? attributeResult,
        bool isWithinSpec,
        int? defectCodeId,
        int? sampleIndex,
        string? notes,
        string recordedBy)
    {
        return new InspectionResult
        {
            InspectionOrderId = inspectionOrderId,
            CharId = charId,
            MeasuredValue = measuredValue,
            AttributeResult = attributeResult,
            IsWithinSpec = isWithinSpec,
            DefectCodeId = defectCodeId,
            SampleIndex = sampleIndex,
            Notes = notes,
            RecordedBy = recordedBy,
            RecordedAt = DateTimeOffset.UtcNow,
        };
    }
}
