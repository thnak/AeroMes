using AeroMes.Domain.Common;

namespace AeroMes.Domain.Master;

public class MoldMachineCompatibility : Entity
{
    public string MoldCode { get; private set; } = string.Empty;
    public string MachineCode { get; private set; } = string.Empty;
    public bool IsCompatible { get; private set; } = true;
    public string? Notes { get; private set; }

    private MoldMachineCompatibility() { }

    public static MoldMachineCompatibility Create(string moldCode, string machineCode, bool isCompatible = true, string? notes = null)
        => new()
        {
            MoldCode = moldCode.Trim().ToUpperInvariant(),
            MachineCode = machineCode.Trim().ToUpperInvariant(),
            IsCompatible = isCompatible,
            Notes = notes?.Trim(),
        };

    public void UpdateCompatibility(bool isCompatible, string? notes)
    {
        IsCompatible = isCompatible;
        Notes = notes?.Trim();
    }
}
