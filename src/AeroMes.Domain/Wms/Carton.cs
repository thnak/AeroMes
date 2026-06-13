using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Wms;

public class Carton : AuditableEntity
{
    public int CartonId { get; private set; }
    public int ShipmentId { get; private set; }
    public string CartonCode { get; private set; } = string.Empty;
    public decimal? GrossWeightKg { get; private set; }
    public CartonStatus Status { get; private set; } = CartonStatus.Open;

    private readonly List<CartonContent> _contents = [];
    public IReadOnlyList<CartonContent> Contents => _contents.AsReadOnly();

    private Carton() { }

    public static Carton Create(int shipmentId, string cartonCode, string? createdBy)
    {
        return new Carton
        {
            ShipmentId = shipmentId,
            CartonCode = cartonCode.Trim().ToUpperInvariant(),
            Status = CartonStatus.Open,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
        };
    }

    public CartonContent AddContent(string productCode, string lotNumber, decimal packedQty)
    {
        if (Status != CartonStatus.Open)
            throw new DomainException($"Thùng '{CartonCode}' phải ở trạng thái Mở để thêm hàng.");
        var content = CartonContent.Create(CartonId, productCode, lotNumber, packedQty);
        _contents.Add(content);
        return content;
    }

    public void Seal(decimal? grossWeightKg, string? sealedBy)
    {
        if (Status != CartonStatus.Open)
            throw new DomainException($"Thùng '{CartonCode}' phải ở trạng thái Mở để niêm phong.");
        if (_contents.Count == 0)
            throw new DomainException($"Thùng '{CartonCode}' phải có ít nhất một mặt hàng trước khi niêm phong.");
        GrossWeightKg = grossWeightKg;
        Status = CartonStatus.Sealed;
        Touch(sealedBy);
    }

    public void MarkShipped(string? updatedBy)
    {
        if (Status != CartonStatus.Sealed)
            throw new DomainException($"Thùng '{CartonCode}' phải được niêm phong trước khi xuất hàng.");
        Status = CartonStatus.Shipped;
        Touch(updatedBy);
    }
}
