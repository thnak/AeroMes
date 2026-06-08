using AeroMes.Domain.Common;
using AeroMes.Domain.Quality;

namespace AeroMes.Domain.Production;

public class DefectDetail : Entity
{
    public long DefectDetailID { get; private set; }
    public long LogID { get; private set; }
    public int DefectCodeID { get; private set; }
    public int Quantity { get; private set; }

    // EF navigation — for queries only
    public DefectCode? DefectCode { get; private set; }

    private DefectDetail() { } // EF constructor

    internal DefectDetail(int defectCodeId, int quantity)
    {
        DefectCodeID = defectCodeId;
        Quantity = quantity;
    }
}
