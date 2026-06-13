using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Wms;

public class CartonContent : Entity
{
    public long ContentId { get; private set; }
    public int CartonId { get; private set; }
    public string ProductCode { get; private set; } = string.Empty;
    public string LotNumber { get; private set; } = string.Empty;
    public decimal PackedQty { get; private set; }

    private CartonContent() { }

    internal static CartonContent Create(int cartonId, string productCode, string lotNumber, decimal packedQty)
    {
        if (packedQty <= 0)
            throw new DomainException("Số lượng đóng gói phải lớn hơn 0.");
        return new CartonContent
        {
            CartonId = cartonId,
            ProductCode = productCode.Trim().ToUpperInvariant(),
            LotNumber = lotNumber.Trim(),
            PackedQty = packedQty,
        };
    }
}
