namespace AeroMes.Domain.Entities;

public class DefectCode
{
    public int DefectCodeID { get; set; }
    public string Code { get; set; } = string.Empty;
    public string DefectName { get; set; } = string.Empty;
    public string? DefectCategory { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<DefectDetail> DefectDetails { get; set; } = [];
}
