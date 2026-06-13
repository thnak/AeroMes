using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Traceability;

public enum ParameterDataSource { Manual, PLC, SCADA, LIMS, Sensor, OPC }

public class ProcessParameter : Entity
{
    public long ParameterID { get; private set; }
    public Guid ProcessRecordID { get; private set; }
    public string ParameterName { get; private set; } = string.Empty;
    public string? NominalValue { get; private set; }
    public string? ActualValue { get; private set; }
    public string? UoM { get; private set; }
    public string? LSL { get; private set; }
    public string? USL { get; private set; }
    public bool? InSpec { get; private set; }
    public DateTime CapturedAt { get; private set; }
    public ParameterDataSource DataSource { get; private set; } = ParameterDataSource.Manual;

    public ProcessRecord? ProcessRecord { get; private set; }

    private ProcessParameter() { }

    internal static ProcessParameter Capture(
        Guid processRecordId,
        string parameterName,
        string? nominalValue,
        string? actualValue,
        string? uom,
        string? lsl,
        string? usl,
        bool? inSpec,
        ParameterDataSource dataSource)
    {
        return new ProcessParameter
        {
            ProcessRecordID = processRecordId,
            ParameterName = parameterName.Trim(),
            NominalValue = nominalValue?.Trim(),
            ActualValue = actualValue?.Trim(),
            UoM = uom?.Trim(),
            LSL = lsl?.Trim(),
            USL = usl?.Trim(),
            InSpec = inSpec,
            CapturedAt = DateTime.UtcNow,
            DataSource = dataSource,
        };
    }
}
