namespace AeroMes.Domain.Exceptions;

public class MoldMachineIncompatibleException(string moldCode, string machineCode)
    : Exception($"Mold '{moldCode}' is not compatible with machine '{machineCode}'. Add a compatibility record first.")
{
    public string MoldCode { get; } = moldCode;
    public string MachineCode { get; } = machineCode;
}
