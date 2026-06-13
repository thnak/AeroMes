using AeroMes.Domain.Common;

namespace AeroMes.Domain.Cost;

public class QualityCostSummary : Entity
{
    public int SummaryID { get; private set; }
    public short PeriodYear { get; private set; }
    public byte PeriodMonth { get; private set; }
    public string? ProductCode { get; private set; }
    public int? WorkCenterID { get; private set; }

    public decimal PreventionCost { get; private set; }
    public decimal AppraisalCost { get; private set; }
    public decimal InternalScrapCost { get; private set; }
    public decimal ReworkCost { get; private set; }
    public decimal CustomerReturnCost { get; private set; }
    public decimal WarrantyCost { get; private set; }
    public decimal TotalQualityCost { get; private set; }

    public decimal? TotalProductionValue { get; private set; }
    public decimal? CopqPct { get; private set; }

    public DateTime LastRefreshedAt { get; private set; }

    private QualityCostSummary() { }

    public static QualityCostSummary Create(
        short year, byte month,
        string? productCode, int? workCenterId,
        decimal preventionCost, decimal appraisalCost,
        decimal internalScrapCost, decimal reworkCost,
        decimal customerReturnCost, decimal warrantyCost,
        decimal? totalProductionValue)
    {
        var total = preventionCost + appraisalCost + internalScrapCost
                    + reworkCost + customerReturnCost + warrantyCost;
        decimal? pct = (totalProductionValue > 0)
            ? total / totalProductionValue * 100m
            : null;

        return new QualityCostSummary
        {
            PeriodYear = year,
            PeriodMonth = month,
            ProductCode = productCode?.Trim().ToUpperInvariant(),
            WorkCenterID = workCenterId,
            PreventionCost = preventionCost,
            AppraisalCost = appraisalCost,
            InternalScrapCost = internalScrapCost,
            ReworkCost = reworkCost,
            CustomerReturnCost = customerReturnCost,
            WarrantyCost = warrantyCost,
            TotalQualityCost = total,
            TotalProductionValue = totalProductionValue,
            CopqPct = pct,
            LastRefreshedAt = DateTime.UtcNow,
        };
    }

    public void Refresh(
        decimal preventionCost, decimal appraisalCost,
        decimal internalScrapCost, decimal reworkCost,
        decimal customerReturnCost, decimal warrantyCost,
        decimal? totalProductionValue)
    {
        PreventionCost = preventionCost;
        AppraisalCost = appraisalCost;
        InternalScrapCost = internalScrapCost;
        ReworkCost = reworkCost;
        CustomerReturnCost = customerReturnCost;
        WarrantyCost = warrantyCost;
        TotalQualityCost = preventionCost + appraisalCost + internalScrapCost
                           + reworkCost + customerReturnCost + warrantyCost;
        TotalProductionValue = totalProductionValue;
        CopqPct = (totalProductionValue > 0)
            ? TotalQualityCost / totalProductionValue * 100m
            : null;
        LastRefreshedAt = DateTime.UtcNow;
    }
}
