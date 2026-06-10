using AeroMes.Domain.Common;

namespace AeroMes.Domain.Master;

public class ApprovedVendorItem : Entity
{
    public int AvlItemId { get; private set; }
    public string SupplierCode { get; private set; } = string.Empty;
    public string ProductCode { get; private set; } = string.Empty;
    public AvlStatus Status { get; private set; }
    public decimal? UnitPrice { get; private set; }
    public string? CurrencyCode { get; private set; }
    public int? LeadTimeDays { get; private set; }
    public decimal? MinOrderQty { get; private set; }
    public string? AqlLevel { get; private set; }
    public bool IsPreferred { get; private set; }
    public DateOnly? ApprovedFrom { get; private set; }
    public DateOnly? ApprovedTo { get; private set; }
    public string? Notes { get; private set; }

    public Product? Product { get; private set; }

    private ApprovedVendorItem() { }

    internal static ApprovedVendorItem Create(
        string supplierCode, string productCode, AvlStatus status,
        decimal? unitPrice, string? currencyCode, int? leadTimeDays,
        decimal? minOrderQty, string? aqlLevel, bool isPreferred,
        DateOnly? approvedFrom, DateOnly? approvedTo, string? notes)
    {
        return new ApprovedVendorItem
        {
            SupplierCode = supplierCode,
            ProductCode = productCode.Trim().ToUpperInvariant(),
            Status = status,
            UnitPrice = unitPrice,
            CurrencyCode = currencyCode?.Trim().ToUpperInvariant(),
            LeadTimeDays = leadTimeDays,
            MinOrderQty = minOrderQty,
            AqlLevel = aqlLevel?.Trim(),
            IsPreferred = isPreferred,
            ApprovedFrom = approvedFrom,
            ApprovedTo = approvedTo,
            Notes = notes?.Trim(),
        };
    }

    internal void Update(
        AvlStatus status, decimal? unitPrice, string? currencyCode,
        int? leadTimeDays, decimal? minOrderQty, string? aqlLevel, bool isPreferred,
        DateOnly? approvedFrom, DateOnly? approvedTo, string? notes)
    {
        Status = status;
        UnitPrice = unitPrice;
        CurrencyCode = currencyCode?.Trim().ToUpperInvariant();
        LeadTimeDays = leadTimeDays;
        MinOrderQty = minOrderQty;
        AqlLevel = aqlLevel?.Trim();
        IsPreferred = isPreferred;
        ApprovedFrom = approvedFrom;
        ApprovedTo = approvedTo;
        Notes = notes?.Trim();
    }
}
