using AeroMes.Domain.Common;

namespace AeroMes.Domain.Quality;

public class VoucherDefectDetail : Entity
{
    public int DetailID { get; private set; }
    public int VoucherID { get; private set; }
    public int DefectCodeId { get; private set; }
    public string DefectName { get; private set; } = string.Empty;
    public decimal Quantity { get; private set; }

    private VoucherDefectDetail() { }

    public static VoucherDefectDetail Create(int voucherId, int defectCodeId, string defectName, decimal quantity)
        => new()
        {
            VoucherID = voucherId,
            DefectCodeId = defectCodeId,
            DefectName = defectName.Trim(),
            Quantity = quantity,
        };
}
