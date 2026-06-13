using AeroMes.Domain.Common;

namespace AeroMes.Domain.Production;

public class MoldAssignment : Entity
{
    public int AssignmentID { get; private set; }
    public string MoldCode { get; private set; } = string.Empty;
    public string MachineCode { get; private set; } = string.Empty;
    public int WOID { get; private set; }
    public DateTime MountedAt { get; private set; }
    public DateTime? UnmountedAt { get; private set; }
    public string MountedBy { get; private set; } = string.Empty;

    private MoldAssignment() { }

    public static MoldAssignment Create(string moldCode, string machineCode, int woid, string mountedBy)
        => new()
        {
            MoldCode = moldCode.Trim().ToUpperInvariant(),
            MachineCode = machineCode.Trim().ToUpperInvariant(),
            WOID = woid,
            MountedAt = DateTime.UtcNow,
            MountedBy = mountedBy.Trim(),
        };

    public void Unmount()
    {
        UnmountedAt = DateTime.UtcNow;
    }
}
