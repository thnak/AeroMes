namespace AeroMes.Domain.Entities;

public class DefectDetail
{
    public long DefectDetailID { get; set; }
    public long LogID { get; set; }
    public int DefectCodeID { get; set; }
    public int Quantity { get; set; }

    public ProductionLog ProductionLog { get; set; } = null!;
    public DefectCode DefectCode { get; set; } = null!;
}
