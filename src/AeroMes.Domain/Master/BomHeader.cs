using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Master;

public enum BomStatus { Draft, UnderReview, Approved, Active, Superseded, Obsolete }
public enum BomType { Production, Trial, Subcontracting }

/// <summary>
/// Versioned BOM header. Only one version per product can be Active at a time;
/// activating a new version supersedes the previous active one.
/// The flat BomItem table remains as the legacy single-version BOM until
/// work-order resolution (#12) switches to versioned headers.
/// </summary>
public class BomHeader : AuditableEntity
{
    public int BomHeaderId { get; private set; }
    public string ProductCode { get; private set; } = string.Empty;
    public string Version { get; private set; } = string.Empty;   // '1.0', '2.0', 'A', 'B'...
    public BomStatus Status { get; private set; } = BomStatus.Draft;
    public BomType BomType { get; private set; } = BomType.Production;
    public bool IsDefault { get; private set; }
    public DateOnly? EffectiveFrom { get; private set; }
    public DateOnly? EffectiveTo { get; private set; }            // null = still current
    public decimal BaseQuantity { get; private set; } = 1m;       // lines produce X units of the parent
    public string? EcoReference { get; private set; }             // EC number that created this version
    public string? ApprovedBy { get; private set; }
    public DateTime? ApprovedAt { get; private set; }
    public string? Notes { get; private set; }

    // EF navigation
    public Product? Product { get; private set; }

    private readonly List<BomLine> _lines = [];
    public IReadOnlyList<BomLine> Lines => _lines.AsReadOnly();

    private readonly List<BomByProduct> _byProducts = [];
    public IReadOnlyList<BomByProduct> ByProducts => _byProducts.AsReadOnly();

    private BomHeader() { }

    public static BomHeader Create(
        string productCode, string version, BomType bomType, decimal baseQuantity,
        string? ecoReference, string? notes, string? createdBy)
    {
        if (baseQuantity <= 0)
            throw new DomainException("Số lượng cơ sở (BaseQuantity) phải lớn hơn 0.");

        return new BomHeader
        {
            ProductCode = productCode.Trim().ToUpperInvariant(),
            Version = version.Trim(),
            Status = BomStatus.Draft,
            BomType = bomType,
            BaseQuantity = baseQuantity,
            EcoReference = ecoReference?.Trim(),
            Notes = notes,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
        };
    }

    public void SetAsDefault(string? updatedBy)
    {
        IsDefault = true;
        Touch(updatedBy);
    }

    public void ClearDefault(string? updatedBy)
    {
        IsDefault = false;
        Touch(updatedBy);
    }

    public void ReplaceByProducts(
        IReadOnlyList<(string ByProductCode, decimal Quantity, string UoMCode, string? Notes)> byProducts,
        string? updatedBy)
    {
        EnsureStatus(BomStatus.Draft, "chỉnh sửa sản phẩm phụ");
        _byProducts.Clear();
        foreach (var bp in byProducts)
            _byProducts.Add(BomByProduct.Create(BomHeaderId, bp.ByProductCode, bp.Quantity, bp.UoMCode, bp.Notes));
        Touch(updatedBy);
    }

    public void ReplaceLines(
        IReadOnlyList<(int LineNo, string ComponentCode, decimal RequiredQty, string UoMCode,
            decimal ScrapFactor, bool IsPhantom, string? Notes)> lines,
        string? updatedBy)
    {
        EnsureStatus(BomStatus.Draft, "chỉnh sửa dòng nguyên liệu");

        foreach (var line in lines)
        {
            var componentCode = line.ComponentCode.Trim().ToUpperInvariant();
            if (componentCode == ProductCode)
                throw new DomainException("BOM không được chứa chính sản phẩm cha làm nguyên liệu.");
        }

        _lines.Clear();
        foreach (var line in lines)
            _lines.Add(BomLine.Create(
                BomHeaderId, line.LineNo, line.ComponentCode, line.RequiredQty,
                line.UoMCode, line.ScrapFactor, line.IsPhantom, line.Notes));
        Touch(updatedBy);
    }

    public void CloneLinesFrom(BomHeader source)
    {
        EnsureStatus(BomStatus.Draft, "sao chép dòng nguyên liệu");
        _lines.Clear();
        foreach (var line in source._lines)
            _lines.Add(BomLine.Create(
                BomHeaderId, line.LineNo, line.ComponentCode, line.RequiredQty,
                line.UoMCode, line.ScrapFactor, line.IsPhantom, line.Notes));
        _byProducts.Clear();
        foreach (var bp in source._byProducts)
            _byProducts.Add(BomByProduct.Create(BomHeaderId, bp.ByProductCode, bp.Quantity, bp.UoMCode, bp.Notes));
    }

    public void SubmitForReview(string? updatedBy)
    {
        EnsureStatus(BomStatus.Draft, "gửi duyệt");
        if (_lines.Count == 0)
            throw new DomainException(
                $"BOM '{ProductCode}' phiên bản '{Version}' phải có ít nhất một dòng nguyên liệu trước khi gửi duyệt.");
        Status = BomStatus.UnderReview;
        Touch(updatedBy);
    }

    public void Approve(string? approvedBy)
    {
        EnsureStatus(BomStatus.UnderReview, "phê duyệt");
        Status = BomStatus.Approved;
        ApprovedBy = approvedBy;
        ApprovedAt = DateTime.UtcNow;
        Touch(approvedBy);
    }

    public void Activate(DateOnly? effectiveFrom, string? updatedBy)
    {
        EnsureStatus(BomStatus.Approved, "kích hoạt");
        Status = BomStatus.Active;
        EffectiveFrom = effectiveFrom ?? DateOnly.FromDateTime(DateTime.UtcNow);
        EffectiveTo = null;
        Touch(updatedBy);
    }

    public void Supersede(string? updatedBy)
    {
        EnsureStatus(BomStatus.Active, "thay thế");
        Status = BomStatus.Superseded;
        EffectiveTo = DateOnly.FromDateTime(DateTime.UtcNow);
        Touch(updatedBy);
    }

    private void EnsureStatus(BomStatus required, string operation)
    {
        if (Status != required)
            throw new DomainException(
                $"BOM '{ProductCode}' phiên bản '{Version}' phải ở trạng thái {required} để {operation}. " +
                $"Trạng thái hiện tại: {Status}.");
    }
}
