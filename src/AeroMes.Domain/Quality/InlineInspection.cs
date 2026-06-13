using AeroMes.Domain.Common;
using AeroMes.Domain.Quality.Events;

namespace AeroMes.Domain.Quality;

public class InlineInspection : Entity
{
    private readonly List<InlineInspectionDefect> _defects = [];

    public long InspectionID { get; private set; }
    public int WOID { get; private set; }
    public int WorkCenterID { get; private set; }
    public string StyleCode { get; private set; } = string.Empty;
    public string? ColorCode { get; private set; }
    public string InspectorID { get; private set; } = string.Empty;
    public string ShiftCode { get; private set; } = string.Empty;
    public int SampleSize { get; private set; }
    public int TotalDefects { get; private set; }
    public decimal DHU { get; private set; }
    public decimal DHU_Target { get; private set; } = 2.5m;
    public bool IsAboveTarget { get; private set; }
    public DateTime InspectedAt { get; private set; }
    public string? Notes { get; private set; }
    public IReadOnlyList<InlineInspectionDefect> Defects => _defects.AsReadOnly();

    private InlineInspection() { }

    public static InlineInspection Create(
        int woid, int workCenterId, string styleCode, string? colorCode,
        string inspectorId, string shiftCode, int sampleSize,
        IEnumerable<(string DefectCode, int Quantity, string? Location, bool IsMajor)> defects,
        decimal dhuTarget = 2.5m, string? notes = null)
    {
        if (sampleSize <= 0) throw new ArgumentException("SampleSize must be > 0.");
        var inspection = new InlineInspection
        {
            WOID = woid,
            WorkCenterID = workCenterId,
            StyleCode = styleCode.Trim().ToUpperInvariant(),
            ColorCode = colorCode?.Trim().ToUpperInvariant(),
            InspectorID = inspectorId.Trim(),
            ShiftCode = shiftCode.Trim(),
            SampleSize = sampleSize,
            DHU_Target = dhuTarget,
            Notes = notes,
            InspectedAt = DateTime.UtcNow,
        };
        foreach (var (code, qty, loc, isMajor) in defects)
        {
            inspection._defects.Add(new InlineInspectionDefect
            {
                DefectCode = code.Trim().ToUpperInvariant(),
                Quantity = qty,
                DefectLocation = loc,
                IsMajor = isMajor,
            });
            inspection.TotalDefects += qty;
        }
        inspection.DHU = sampleSize > 0 ? (decimal)inspection.TotalDefects / sampleSize * 100m : 0m;
        inspection.IsAboveTarget = inspection.DHU > dhuTarget;
        if (inspection.IsAboveTarget)
            inspection.RaiseDomainEvent(new HighDHUAlertEvent(woid, workCenterId, styleCode, inspection.DHU, dhuTarget));
        return inspection;
    }
}

public class InlineInspectionDefect
{
    public long DefectID { get; set; }
    public long InspectionID { get; set; }
    public string DefectCode { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string? DefectLocation { get; set; }
    public bool IsMajor { get; set; }
}
