using AeroMes.Domain.Common;
using AeroMes.Domain.Quality.Events;

namespace AeroMes.Domain.Quality;

public class AQLInspection : Entity
{
    private readonly List<AQLInspectionDefect> _defects = [];

    public int AQLInspectionID { get; private set; }
    public int WOID { get; private set; }
    public string AQLLevel { get; private set; } = "2.5";
    public string InspectionLevel { get; private set; } = "II";
    public int LotSize { get; private set; }
    public int SampleSize { get; private set; }
    public int AcceptanceNumber { get; private set; }
    public int RejectionNumber { get; private set; }
    public int DefectsFound { get; private set; }
    public string Decision { get; private set; } = "PENDING";
    public string InspectorID { get; private set; } = string.Empty;
    public DateTime InspectedAt { get; private set; }
    public string? Notes { get; private set; }
    public IReadOnlyList<AQLInspectionDefect> Defects => _defects.AsReadOnly();

    private AQLInspection() { }

    public static AQLInspection Create(
        int woid, string aqlLevel, string inspectionLevel,
        int lotSize, int sampleSize, int acceptanceNumber, int rejectionNumber,
        string inspectorId,
        IEnumerable<(string DefectCode, int Quantity, bool IsMajor)> defects,
        string? notes = null)
    {
        var inspection = new AQLInspection
        {
            WOID = woid,
            AQLLevel = aqlLevel,
            InspectionLevel = inspectionLevel,
            LotSize = lotSize,
            SampleSize = sampleSize,
            AcceptanceNumber = acceptanceNumber,
            RejectionNumber = rejectionNumber,
            InspectorID = inspectorId.Trim(),
            Notes = notes,
            InspectedAt = DateTime.UtcNow,
        };
        foreach (var (code, qty, isMajor) in defects)
        {
            inspection._defects.Add(new AQLInspectionDefect
            {
                DefectCode = code.Trim().ToUpperInvariant(),
                Quantity = qty,
                IsMajor = isMajor,
            });
            inspection.DefectsFound += qty;
        }
        inspection.Decision = inspection.DefectsFound <= acceptanceNumber
            ? "ACCEPT"
            : inspection.DefectsFound >= rejectionNumber
                ? "REJECT"
                : "PENDING";

        if (inspection.Decision == "REJECT")
            inspection.RaiseDomainEvent(new AQLRejectedEvent(woid, inspection.DefectsFound, rejectionNumber, aqlLevel));
        else if (inspection.Decision == "ACCEPT")
            inspection.RaiseDomainEvent(new AQLAcceptedEvent(woid, aqlLevel));

        return inspection;
    }
}

public class AQLInspectionDefect
{
    public long DefectID { get; set; }
    public int AQLInspectionID { get; set; }
    public string DefectCode { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public bool IsMajor { get; set; }
}
