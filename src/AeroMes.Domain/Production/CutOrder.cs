using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Production.Events;

namespace AeroMes.Domain.Production;

public enum CutOrderStatus { Planned, FabricReserved, Cutting, Completed, Cancelled }

public class CutOrder : Entity
{
    public int CutOrderID { get; private set; }
    public string CutOrderCode { get; private set; } = string.Empty;
    public int WOID { get; private set; }
    public string StyleCode { get; private set; } = string.Empty;
    public string ColorCode { get; private set; } = string.Empty;
    public string FabricProductCode { get; private set; } = string.Empty;
    public string ShadeCode { get; private set; } = string.Empty;
    public string? MarkerReference { get; private set; }
    public decimal? MarkerEfficiencyPct { get; private set; }
    public int PlyCount { get; private set; }
    public decimal SpreadLengthMeters { get; private set; }
    public decimal FabricWidthCm { get; private set; }
    public decimal? EstimatedFabricMeters { get; private set; }
    public decimal? ActualFabricMeters { get; private set; }
    public decimal? FabricWastePct { get; private set; }  // SQL computed column
    public CutOrderStatus Status { get; private set; } = CutOrderStatus.Planned;
    public DateTime? CuttingStartedAt { get; private set; }
    public DateTime? CuttingCompletedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private readonly List<CutOrderLine> _lines = [];
    public IReadOnlyList<CutOrderLine> Lines => _lines.AsReadOnly();

    private readonly List<CutOrderFabricUsage> _fabricUsage = [];
    public IReadOnlyList<CutOrderFabricUsage> FabricUsage => _fabricUsage.AsReadOnly();

    private CutOrder() { }

    public static CutOrder Create(
        string cutOrderCode, int woid, string styleCode, string colorCode,
        string fabricProductCode, string shadeCode, int plyCount,
        decimal spreadLengthMeters, decimal fabricWidthCm,
        IEnumerable<(string SizeCode, int Qty)> lines,
        string? markerReference = null, decimal? estimatedFabricMeters = null)
    {
        if (plyCount <= 0) throw new DomainException("Ply count must be positive.");
        if (spreadLengthMeters <= 0) throw new DomainException("Spread length must be positive.");
        if (fabricWidthCm <= 0) throw new DomainException("Fabric width must be positive.");

        var lineList = lines.ToList();
        if (lineList.Count == 0 || lineList.All(l => l.Qty <= 0))
            throw new DomainException("At least one size line with quantity > 0 is required.");

        var order = new CutOrder
        {
            CutOrderCode = cutOrderCode.Trim().ToUpperInvariant(),
            WOID = woid,
            StyleCode = styleCode.Trim().ToUpperInvariant(),
            ColorCode = colorCode.Trim().ToUpperInvariant(),
            FabricProductCode = fabricProductCode.Trim().ToUpperInvariant(),
            ShadeCode = shadeCode.Trim().ToUpperInvariant(),
            PlyCount = plyCount,
            SpreadLengthMeters = spreadLengthMeters,
            FabricWidthCm = fabricWidthCm,
            MarkerReference = markerReference?.Trim(),
            EstimatedFabricMeters = estimatedFabricMeters,
            CreatedAt = DateTime.UtcNow,
        };

        foreach (var (sizeCode, qty) in lineList.Where(l => l.Qty > 0))
            order._lines.Add(CutOrderLine.Create(sizeCode, qty));

        return order;
    }

    public void ReserveFabric(IReadOnlyList<int> rollIds)
    {
        if (Status != CutOrderStatus.Planned)
            throw new DomainException($"Fabric can only be reserved when status is Planned. Current: {Status}.");

        foreach (var rollId in rollIds)
        {
            if (_fabricUsage.All(u => u.RollID != rollId))
                _fabricUsage.Add(CutOrderFabricUsage.Create(rollId));
        }
        Status = CutOrderStatus.FabricReserved;
    }

    public void StartCutting()
    {
        if (Status != CutOrderStatus.FabricReserved)
            throw new DomainException($"Cutting can only start after fabric is reserved. Current: {Status}.");
        Status = CutOrderStatus.Cutting;
        CuttingStartedAt = DateTime.UtcNow;
    }

    public void CompleteCutting(
        decimal actualFabricMeters, decimal markerEfficiencyPct,
        IEnumerable<(string SizeCode, int QtyCut)> cutResults)
    {
        if (Status != CutOrderStatus.Cutting)
            throw new DomainException($"Cannot complete — cut order is not in Cutting status. Current: {Status}.");
        if (actualFabricMeters <= 0) throw new DomainException("Actual fabric meters must be positive.");
        if (markerEfficiencyPct is < 0 or > 100)
            throw new DomainException("Marker efficiency must be between 0 and 100.");

        ActualFabricMeters = actualFabricMeters;
        MarkerEfficiencyPct = markerEfficiencyPct;
        CuttingCompletedAt = DateTime.UtcNow;
        Status = CutOrderStatus.Completed;

        foreach (var (sizeCode, qtyCut) in cutResults)
        {
            var line = _lines.FirstOrDefault(l => l.SizeCode == sizeCode.Trim().ToUpperInvariant());
            if (line is not null) line.RecordCut(qtyCut);
        }

        if (markerEfficiencyPct < 80m)
            RaiseDomainEvent(new LowMarkerEfficiencyEvent(CutOrderID, CutOrderCode, markerEfficiencyPct));
    }

    public void Cancel()
    {
        if (Status is CutOrderStatus.Completed or CutOrderStatus.Cancelled)
            throw new DomainException($"Cannot cancel a {Status} cut order.");
        Status = CutOrderStatus.Cancelled;
    }
}
