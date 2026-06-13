using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Master;

public class BomByProduct : Entity
{
    public int BomByProductId { get; private set; }
    public int BomHeaderId { get; private set; }
    public string ByProductCode { get; private set; } = string.Empty;
    public decimal Quantity { get; private set; }
    public string UoMCode { get; private set; } = string.Empty;
    public string? Notes { get; private set; }

    public Product? ByProduct { get; private set; }

    private BomByProduct() { }

    internal static BomByProduct Create(
        int bomHeaderId, string byProductCode, decimal quantity, string uomCode, string? notes)
    {
        if (quantity <= 0)
            throw new DomainException($"Số lượng sản phẩm phụ phải lớn hơn 0: {byProductCode}.");

        return new BomByProduct
        {
            BomHeaderId = bomHeaderId,
            ByProductCode = byProductCode.Trim().ToUpperInvariant(),
            Quantity = quantity,
            UoMCode = uomCode.Trim().ToUpperInvariant(),
            Notes = notes,
        };
    }
}
