using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Cost;

public enum DisposalMethod { Scrap, Rework, Salvage, ReturnToSupplier }

public class ScrapTransaction : Entity
{
    public long ScrapTxID { get; private set; }
    public int WOID { get; private set; }
    public long? LogID { get; private set; }
    public int? DefectCodeId { get; private set; }
    public string ProductCode { get; private set; } = string.Empty;
    public string? LotNumber { get; private set; }
    public int ScrapQty { get; private set; }

    public decimal MaterialCostPerUnit { get; private set; }
    public decimal LaborCostSunk { get; private set; }
    public decimal MachineCostSunk { get; private set; }
    public decimal TotalScrapCost { get; private set; }

    public DisposalMethod DisposalMethod { get; private set; } = DisposalMethod.Scrap;
    public int? ScrapLocationId { get; private set; }
    public string? ApprovedBy { get; private set; }
    public DateTime ScrapAt { get; private set; }
    public string? Notes { get; private set; }
    public string? CreatedBy { get; private set; }

    private ScrapTransaction() { }

    public static ScrapTransaction Create(
        int woid, long? logId, int? defectCodeId,
        string productCode, string? lotNumber, int scrapQty,
        decimal materialCostPerUnit, decimal laborCostSunk, decimal machineCostSunk,
        DisposalMethod disposalMethod, int? scrapLocationId,
        string? approvedBy, string? notes, string? createdBy)
    {
        if (string.IsNullOrWhiteSpace(productCode))
            throw new DomainException("Mã sản phẩm không được để trống.");
        if (scrapQty <= 0)
            throw new DomainException("Số lượng phế liệu phải lớn hơn 0.");

        var totalCost = (materialCostPerUnit + laborCostSunk + machineCostSunk) * scrapQty;
        return new ScrapTransaction
        {
            WOID = woid,
            LogID = logId,
            DefectCodeId = defectCodeId,
            ProductCode = productCode.Trim().ToUpperInvariant(),
            LotNumber = lotNumber?.Trim(),
            ScrapQty = scrapQty,
            MaterialCostPerUnit = materialCostPerUnit,
            LaborCostSunk = laborCostSunk,
            MachineCostSunk = machineCostSunk,
            TotalScrapCost = totalCost,
            DisposalMethod = disposalMethod,
            ScrapLocationId = scrapLocationId,
            ApprovedBy = approvedBy?.Trim(),
            ScrapAt = DateTime.UtcNow,
            Notes = notes?.Trim(),
            CreatedBy = createdBy,
        };
    }
}
