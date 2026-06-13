using AeroMes.Domain.Common;

namespace AeroMes.Domain.Quality;

public class NcrDefectLine : Entity
{
    public int LineId { get; private set; }
    public int NcrId { get; private set; }
    public int DefectCodeId { get; private set; }
    public int QtyDefective { get; private set; }
    public string? Notes { get; private set; }

    public Ncr? Ncr { get; private set; }
    public DefectCode? DefectCode { get; private set; }

    private NcrDefectLine() { }

    public static NcrDefectLine Create(int ncrId, int defectCodeId, int qtyDefective, string? notes)
        => new() { NcrId = ncrId, DefectCodeId = defectCodeId, QtyDefective = qtyDefective, Notes = notes };
}
