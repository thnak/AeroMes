using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Quality;

public class DefectDetail : Entity
{
    public long DefectDetailID { get; private set; }
    public long LogID { get; private set; }
    public int DefectCodeID { get; private set; }
    public int Quantity { get; private set; }

    // EF navigation
    public DefectCode? DefectCode { get; private set; }

    private DefectDetail() { }

    internal DefectDetail(int defectCodeId, int quantity)
    {
        if (quantity <= 0)
            throw new DomainException($"Defect quantity must be positive. Got: {quantity}.");
        DefectCodeID = defectCodeId;
        Quantity = quantity;
    }
}
